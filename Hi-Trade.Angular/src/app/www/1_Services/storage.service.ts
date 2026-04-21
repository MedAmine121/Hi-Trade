import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root'
})
export class StorageService {

    constructor() { }

    // LocalStorage methods
    setLocalStorage(key: string, value: unknown): void {
        localStorage.setItem(key, JSON.stringify(value));
    }

    getLocalStorage(key: string): unknown {
        const item = localStorage.getItem(key);
        return item ? JSON.parse(item) : null;
    }

    removeLocalStorage(key: string): void {
        localStorage.removeItem(key);
    }

    clearLocalStorage(): void {
        localStorage.clear();
    }

    // SessionStorage methods
    setSessionStorage(key: string, value: unknown): void {
        sessionStorage.setItem(key, JSON.stringify(value));
    }

    getSessionStorage(key: string): unknown {
        const item = sessionStorage.getItem(key);
        return item ? JSON.parse(item) : null;
    }

    removeSessionStorage(key: string): void {
        sessionStorage.removeItem(key);
    }

    clearSessionStorage(): void {
        sessionStorage.clear();
    }
}