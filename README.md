# Phone-Communication-Hub
Phone Communication Hub platform used to redirect business phone calls to employees or sale associates. Uses postgres database &amp; Vonage phone SDK. The source includes an SDK, Web App, &amp; REST API server. Vonage redirects calls &amp; SMS to the REST API where it is handled by the platform and distributed to a sales associate or employee. The platform is bare bones with minimal functionality but can be forked and continued easily.

--Features--
Login & Registration portal for employees

SMS chatting between employees and callers

Automatic contact saving and message logging

Pop up phone dial menu similar to facebook messenger

Vonage webhooks to redirect callers/texters to other employees

Voice mail recording when no associates are online to take a call

--Needs finishing--
Phone audio and employee mic streaming completely implemented. Raw integration is in there but not functioning correctly
