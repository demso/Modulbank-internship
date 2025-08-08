using BankAccounts.Api.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Api.Infrastructure.Database.Migrator;

public static class DatabaseMigrator
{
    public static async Task MigrateDatabase(this IApplicationBuilder app)
    {
        var context = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<IBankAccountsDbContext>();

        var sql = @"CREATE EXTENSION IF NOT EXISTS btree_gist;";// Добавим расшриение чтобы создать индекс по дате в Transactions

        await context.Database.ExecuteSqlRawAsync(sql);

        await context.Database.MigrateAsync();

        // ReSharper disable once StringLiteralTypo Наименование верно
        sql =
            """
            CREATE OR REPLACE PROCEDURE accrue_interest(
                p_account_id int
            )
            LANGUAGE plpgsql
            AS $$
            DECLARE
                v_account_type account_type;
                v_balance numeric;
                v_interest_rate numeric;
                v_interest_amount numeric;
                v_new_balance numeric;
                v_close_date TIMESTAMP;
            BEGIN
                -- Получаем информацию о счёте
                SELECT "AccountType", "Balance", "InterestRate", "CloseDate"
                INTO v_account_type, v_balance, v_interest_rate, v_close_date
                FROM "Accounts"
                WHERE "AccountId" = p_account_id
                FOR UPDATE; -- Блокируем строку для предотвращения конфликтов
            
                -- Проверяем, что счёт найден
                IF NOT FOUND THEN
                    RAISE EXCEPTION 'Счёт с ID % не найден', p_account_id;
                END IF;
            
                IF v_close_date IS NOT NULL THEN
                    RAISE NOTICE 'Счёт % закрыт', p_account_id;
                    RETURN;
                END IF;
            
                -- Проверяем, что это счёт с начислением процентов
                IF v_interest_rate IS NULL OR v_interest_rate <= 0 THEN
                    RAISE NOTICE 'Счёт % не поддерживает начисление процентов', p_account_id;
                    RETURN;
                END IF;
            
                -- Проверяем, есть ли задолженность по кредитному счету
                IF v_account_type = 'credit' AND v_balance > 0 THEN
                    RAISE NOTICE 'Положмтельный баланс на счету % начислять проценты не нужно', p_account_id;
                    RETURN;
                END IF;
            
                -- Вычисляем проценты (проценты начисляются каждый день, interestRate - годовые проценты)
                v_interest_amount := v_balance * (v_interest_rate / 365) / 100;
            
                -- Округляем до 2 знаков после запятой
                v_interest_amount := ROUND(v_interest_amount, 2);
            
                -- Проверяем, что сумма не равна 0
                IF v_interest_amount = 0 THEN
                    RAISE NOTICE 'Начисленные проценты по счёту % равны 0', p_account_id;
                    RETURN;
                END IF;
            
                -- Вычисляем новый баланс
                v_new_balance := v_balance + v_interest_amount;
            
                -- Обновляем баланс счёта
                UPDATE "Accounts"
                SET "Balance" = v_new_balance
                WHERE "AccountId" = p_account_id;
            
                -- Записываем транзакцию начисления процентов
                INSERT INTO "Transactions" (
                    "TransactionId",
                    "AccountId",
                    "CounterpartyAccountId",
                    "Amount",
                    "Currency",
                    "DateTime",
                    "Description",
                    "TransactionType"
                )
                VALUES (
                    gen_random_uuid(),
                    p_account_id,
            		0,
                    v_interest_amount,
                    (SELECT "Currency" FROM "Accounts" WHERE "AccountId" = p_account_id),
                    NOW(),
                    'Начисление процентов по вкладу',
                    CASE 
                        WHEN v_account_type = 'credit' THEN 'credit'::transaction_type 
                        WHEN v_account_type = 'deposit' THEN 'debit'::transaction_type
                        WHEN v_account_type = 'checking' THEN 'debit'::transaction_type
                        ELSE 'debit' 
                    END
                );
            
                RAISE NOTICE 'Начислены проценты по счёту %: %', p_account_id, v_interest_amount;
            
            END;
            $$;
            """;

        await context.Database.ExecuteSqlRawAsync(sql);
    }
}