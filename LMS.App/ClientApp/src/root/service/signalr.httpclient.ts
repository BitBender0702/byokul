
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

import { AbortError, HttpError, TimeoutError } from "@aspnet/signalr";
import { HttpClient, HttpRequest, HttpResponse } from "@aspnet/signalr";
import { ILogger, LogLevel } from "@aspnet/signalr";

export class CustomXhrHttpClient extends HttpClient {
    private readonly logger: ILogger;
    private readonly token: string;

    public constructor(token:string, logger?: ILogger) {
        super();
        this.logger = logger == null ? new Object as ILogger : logger; 
    this.token = token;
    }

    /** @inheritDoc */
    public send(request: HttpRequest): Promise<HttpResponse> {
        // Check that abort was not signaled before calling send
        if (request.abortSignal && request.abortSignal.aborted) {
            return Promise.reject(new AbortError());
        }

        if (!request.method) {
            return Promise.reject(new Error("No method defined."));
        }
        if (!request.url) {
            return Promise.reject(new Error("No url defined."));
        }

        return new Promise<HttpResponse>((resolve, reject) => {
            const xhr = new XMLHttpRequest();
            xhr.open(request.method!, request.url!, true);
            xhr.withCredentials = false;
            xhr.setRequestHeader("X-Requested-With", "XMLHttpRequest");
            // Explicitly setting the Content-Type header for React Native on Android platform.
            xhr.setRequestHeader("Content-Type", "text/plain;charset=UTF-8");
            xhr.setRequestHeader("Authorization", `Bearer ${this.token}`);

            const headers = request.headers;
            if (headers) {
                Object.keys(headers)
                    .forEach((header) => {
                        xhr.setRequestHeader(header, headers[header]);
                    });
            }

            if (request.responseType) {
                xhr.responseType = request.responseType;
            }

            if (request.abortSignal) {
                request.abortSignal.onabort = () => {
                    xhr.abort();
                    reject(new AbortError());
                };
            }

            if (request.timeout) {
                xhr.timeout = request.timeout;
            }

            xhr.onload = () => {
                if (request.abortSignal) {
                    request.abortSignal.onabort = null;
                }

                if (xhr.status >= 200 && xhr.status < 300) {
                    resolve(new HttpResponse(xhr.status, xhr.statusText, xhr.response || xhr.responseText));
                } else {
                    reject(new HttpError(xhr.statusText, xhr.status));
                }
            };

            xhr.onerror = () => {
                if(this.logger){
                    this.logger.log(LogLevel.Warning, `Error from HTTP request. ${xhr.status}: ${xhr.statusText}.`);
                }               
                reject(new HttpError(xhr.statusText, xhr.status));
            };

            xhr.ontimeout = () => {
                if(this.logger){
                this.logger.log(LogLevel.Warning, `Timeout from HTTP request.`);
                }
                reject(new TimeoutError());
            };

             xhr.send(request.content || "");
        });
    }
}

