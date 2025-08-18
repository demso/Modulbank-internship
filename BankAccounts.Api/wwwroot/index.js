/* Source: https://gist.github.com/lamberta/3768814
 * Parse a string function definition and return a function object. Does not use eval.
 * @param {string} str
 * @return {function}
 *
 * Example:
 *  var f = function (x, y) { return x * y; };
 *  var g = parseFunction(f.toString());
 *  g(33, 3); //=> 99
 */

function parseFunction(str) {
    if (!str) return void (0);

    var fn_body_idx = str.indexOf('{'),
        fn_body = str.substring(fn_body_idx + 1, str.lastIndexOf('}')),
        fn_declare = str.substring(0, fn_body_idx),
        fn_params = fn_declare.substring(fn_declare.indexOf('(') + 1, fn_declare.lastIndexOf(')')),
        args = fn_params.split(',');

    args.push(fn_body);

    function Fn() {
        return Function.apply(this, args);
    }
    Fn.prototype = Function.prototype;

    return new Fn();
}

window.onload = function () {
    
    
    var configObject = JSON.parse('{"urls":[{"url":"swagger-ui/swagger.json","name":"Bank Accounts API (Static Spec) v1"}],"deepLinking":false,"persistAuthorization":false,"displayOperationId":false,"defaultModelsExpandDepth":1,"defaultModelExpandDepth":1, "defaultModelRendering":"example","displayRequestDuration":false,"docExpansion":"list","showExtensions":false,"showCommonExtensions":false,"supportedSubmitMethods":["get","put","post","delete","options","head","patch","trace"],"tryItOutEnabled":false}');
    var oauthConfigObject = JSON.parse('{"scopeSeparator":" ","scopes":[],"useBasicAuthenticationWithAccessCodeGrant":false,"usePkceWithAuthorizationCodeGrant":false}');

    // Workaround for https://github.com/swagger-api/swagger-ui/issues/5945
    configObject.urls.forEach(function (item) {
        if (item.url.startsWith("http") || item.url.startsWith("/")) return;
        item.url = window.location.href.replace("index.html", item.url).split('#')[0];
    });

    // If validatorUrl is not explicitly provided, disable the feature by setting to null
    if (!configObject.hasOwnProperty("validatorUrl"))
        configObject.validatorUrl = null

    // If oauth2RedirectUrl isn't specified, use the built-in default
    if (!configObject.hasOwnProperty("oauth2RedirectUrl"))
        configObject.oauth2RedirectUrl = (new URL("oauth2-redirect.html", window.location.href)).href;

    // Apply mandatory parameters
    configObject.dom_id = "#swagger-ui";
    configObject.presets = [SwaggerUIBundle.presets.apis, SwaggerUIStandalonePreset];
    configObject.layout = "StandaloneLayout";

    // Parse and add interceptor functions
    var interceptors = JSON.parse('{}');
    if (interceptors.RequestInterceptorFunction)
        configObject.requestInterceptor = parseFunction(interceptors.RequestInterceptorFunction);
    if (interceptors.ResponseInterceptorFunction)
        configObject.responseInterceptor = parseFunction(interceptors.ResponseInterceptorFunction);
    
    if (configObject.plugins) {
        configObject.plugins = configObject.plugins.map(eval);
    }

    function modifyModelsList() {
        console.log("Попытка модификации списка схем...");
        const modelsContainer = document.querySelector('.models'); // Находим контейнер схем
        if (!modelsContainer) {
            console.warn('Контейнер схем .models не найден');
            return;
        }

        const modelElements = modelsContainer.querySelectorAll('.model-container'); // Находим все элементы схем
        if (modelElements.length === 0) {
            console.warn('Элементы схем .model-container не найдены');
            // Возможно, они еще не загрузились, попробуем позже
            return;
        }

        console.log(`Найдено ${modelElements.length} схем.`);

        // --- ЛОГИКА ГРУППИРОВКИ ---
        const eventElements = [];
        const otherElements = [];
        let lastElement = null;

        modelElements.forEach((el, index) => {
            console.log(`Обработка схемы #${index + 1}...`);
            let modelName = "Unknown Schema";

            // --- Более надежный способ получения имени схемы ---
            try {
                // Метод 1: Попробуем найти элемент с именем схемы напрямую
                // В разных версиях структура может отличаться
                let modelTitleElement = el.querySelector('.model .model-title'); // Часто используется
                if (!modelTitleElement) {
                    modelTitleElement = el.querySelector('.model-title'); // Более общий поиск
                }
                if (!modelTitleElement) {
                    modelTitleElement = el.querySelector('.model-box .model-title'); // Ещё одна возможная структура
                }
                if (!modelTitleElement) {
                    modelTitleElement = el.querySelector('.model-box summary .model-title'); // Если внутри <summary>
                }

                // Если всё ещё не найден, попробуем получить атрибут data-model-name
                if (!modelTitleElement) {
                    modelName = el.getAttribute('data-model-name') || `Unnamed Schema ${index}`;
                    console.warn(`Не найден элемент .model-title для схемы #${index + 1}, использую data-model-name или запасное имя: ${modelName}`);
                } else {
                    modelName = modelTitleElement.textContent?.trim() || `Unnamed Schema ${index}`;
                    console.log(`Имя схемы #${index + 1}: ${modelName}`);
                }
            } catch (e) {
                console.error(`Ошибка при получении имени схемы #${index + 1}:`, e);
                modelName = `Error Schema ${index}`;
            }

            // Определяем, является ли схема "событием"
            // Убедитесь, что логика соответствует ИМЕНАМ ВАШИХ СХЕМ в swagger.json
            // Например, если вы НЕ переименовывали ключи в swagger.json, используйте оригинальные имена:
            const isEvent = modelName.startsWith("Client") ||
                modelName.startsWith("Money") ||
                modelName === "Metadata" ||
                modelName === "AccountOpened" ||
                modelName === "TransferCompleted" ||
                modelName === "InterestAccrued";
            // ИЛИ, если вы переименовали ключи в swagger.json на "!![Событие] Имя":
            // const isEvent = modelName.startsWith("!![Событие] ");
            // ИЛИ, если вы переименовали ключи на "![Событие] Имя":
            // const isEvent = modelName.startsWith("![Событие] ");

            console.log(`Схема "${modelName}" классифицирована как ${isEvent ? 'Событие' : 'Общая'}`);

            if (isEvent) {
                eventElements.push({element: el, name: modelName});
            } else {
                otherElements.push({element: el, name: modelName});
            }
            lastElement = el;
        });

        try {
            // 1. Создадим функцию для создания заголовка группы
            function createGroupHeader(title) {
                const header = document.createElement('h5');
                header.textContent = title;
                header.style.marginTop = '20px';
                header.style.marginBottom = '10px';
                header.style.fontWeight = 'bold';
                header.style.color = '#3b4151'; // Стандартный цвет заголовков Swagger UI
                header.classList.add('custom-models-group-header');
                console.log(`Создан заголовок группы: ${title}`);
                return header;
            }

            // 2. Обработка "Общих" схем
            if (otherElements.length > 0) {
                console.log(`Обработка ${otherElements.length} 'Общих' схем...`);
                const firstOtherElement = otherElements[0].element;
                const generalHeader = createGroupHeader('Общие');
                // Вставляем заголовок перед первой "Общей" схемой
                firstOtherElement.parentNode.insertBefore(generalHeader, firstOtherElement);
                console.log("Заголовок 'Общие' вставлен.");
            } else {
                console.log("Нет 'Общих' схем для обработки.");
            }

            // 3. Обработка "Событий"
            if (eventElements.length > 0) {
                console.log(`Обработка ${eventElements.length} 'Событий'...`);
                const eventsHeader = createGroupHeader('События');
                // Определяем, куда вставить заголовок "События"
                let insertAfterElement = null;
                if (otherElements.length > 0) {
                    // Вставляем после последней "Общей" схемы
                    insertAfterElement = otherElements[otherElements.length - 1].element;
                } else if (modelElements.length > 0) {
                    // Если нет "Общих", вставляем после последней схемы в общем списке (которая может быть событием)
                    insertAfterElement = modelElements[modelElements.length - 1];
                } else {
                    // Если вообще нет схем (маловероятно), вставляем в начало контейнера
                    modelsContainer.insertBefore(eventsHeader, modelsContainer.firstChild);
                    console.log("Заголовок 'События' вставлен в начало (нет других схем).");
                    // В этом случае не нужно перемещать элементы, просто выходим
                    return;
                }

                if (insertAfterElement && insertAfterElement.nextSibling) {
                    insertAfterElement.parentNode.insertBefore(eventsHeader, insertAfterElement.nextSibling);
                } else if (insertAfterElement) {
                    // Если nextSibling null, добавляем в конец родителя
                    insertAfterElement.parentNode.appendChild(eventsHeader);
                }
                console.log("Заголовок 'События' вставлен.");

                // Перемещаем элементы событий после заголовка "События"
                // Так как мы вставили заголовок после определенного элемента,
                // нам нужно переместить все элементы событий в конец контейнера
                // Это проще и надежнее, чем вставка после конкретного элемента
                eventElements.forEach(({element, name}) => {
                    console.log(`Перемещаю схему события: ${name}`);
                    modelsContainer.appendChild(element); // appendChild перемещает существующий элемент
                });
                console.log("Все элементы 'Событий' перемещены в конец.");

            } else {
                console.log("Нет 'Событий' для обработки.");
            }

            console.log('Список схем успешно модифицирован: "Общие" и "События" разделены.');
        } catch (renderError) {
            console.error("Ошибка при рендеринге/перемещении элементов:", renderError);
        }

        console.log('Список схем модифицирован: "Общие" и "События" разделены.');
    }
    
    // Begin Swagger UI call region

    const ui = SwaggerUIBundle({
        // ... ваши существующие опции ...
        url: "/swagger/v1/swagger.json", // Убедитесь, что путь правильный к вашему статическому файлу
        dom_id: '#swagger-ui',
        deepLinking: true,
        presets: [
            SwaggerUIBundle.presets.apis,
            SwaggerUIStandalonePreset
        ],
        plugins: [
            SwaggerUIBundle.plugins.DownloadUrl
            // НЕ добавляем CustomComponentsPlugin, так как он не работает
        ],
        layout: "StandaloneLayout",
        // requestInterceptor: function(request) {
        //     // Получаем токен из localStorage (или другого места, где он хранится)
        //     // Swagger UI по умолчанию сохраняет его там под ключом "authorized"
        //     let authToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIxZGUzNWZiZi1kNDEwLTQyMDYtODM4NC03MGRhMGJkMDM4MGIiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoidGVzdCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiZTU4ZjRhYTktYjFjYy1hNjViLTljNGMtMDg3M2QzOTFlOTg3IiwiZXhwIjoyNTEyODU0MzYyLCJpc3MiOiJCYW5rQWNjb3VudEF1dGhvcml6YXRpb24iLCJhdWQiOiJCYW5rQWNjb3VudHNXZWJBUEkifQ.QJG-5p1bj5KZdpxvcCvDII6-fiC3gWRqMKH12WKNIM0";
        //
        //     // Если токен всё ещё не найден, можно попробовать получить его из другого места,
        //     // например, из кастомного input на странице или cookie (менее распространено для bearer)
        //     // if (!authToken) {
        //     //   authToken = document.getElementById('custom-token-input')?.value;
        //     // }
        //
        //     // Если токен найден, добавляем его в заголовки запроса
        //     if (authToken) {
        //         // Убедимся, что заголовки существуют
        //         request.headers = request.headers || {};
        //         // Добавляем заголовок Authorization
        //         // Проверяем, не начинается ли токен уже с "Bearer "
        //         if (request.headers["Authorization"])
        //         if (!authToken.startsWith("Bearer ")) {
        //             request.headers["Authorization"] = "Bearer " + authToken;
        //         } else {
        //             request.headers["Authorization"] = authToken;
        //         }
        //         console.log("Добавлен Bearer токен к запросу:", request.url);
        //     } else {
        //         console.log("Bearer токен не найден, запрос отправляется без него:", request.url);
        //     }
        //
        //     // Важно: вернуть изменённый объект request
        //     return request;
        // },
        // --- Настройка для наблюдения за изменениями DOM ---
        onComplete: function() {
            console.log("Swagger UI загружен. Применяем кастомизацию...");
            // Попробуем применить кастомизацию сразу
            modifyModelsList();

            // Иногда список схем может подгружаться асинхронно.
            // Добавим MutationObserver для отслеживания появления элементов.
            const observer = new MutationObserver(function(mutations) {
                mutations.forEach(function(mutation) {
                    if (mutation.type === 'childList') {
                        // Проверим, появились ли новые элементы .model-container
                        for (let i = 0; i < mutation.addedNodes.length; i++) {
                            const node = mutation.addedNodes[i];
                            if (node.nodeType === Node.ELEMENT_NODE) {
                                if (node.classList.contains('model-container') || node.querySelector('.model-container')) {
                                    console.log('Обнаружены новые схемы, переприменяем кастомизацию...');
                                    // Небольшая задержка, чтобы убедиться, что все схемы загрузились
                                    setTimeout(modifyModelsList, 100);
                                    // Отключаем observer после первой успешной модификации, чтобы не реагировать на дальнейшие изменения
                                    // observer.disconnect();
                                    // break;
                                }
                            }
                        }
                    }
                });
            });

            const modelsContainer = document.querySelector('.models');
            if (modelsContainer) {
                observer.observe(modelsContainer, { childList: true, subtree: true });
                console.log("MutationObserver запущен для .models");
            } else {
                console.warn("Контейнер .models не найден для наблюдения");
            }
        }
    });

    ui.initOAuth(oauthConfigObject);

    // End Swagger UI call region

    window.ui = ui
}