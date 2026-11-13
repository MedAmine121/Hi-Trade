import { Injectable } from "@angular/core";
import { BaseResult } from "../2_Models/common/base-result.model";
import { SaveResponse } from "../2_Models/common/save-response.model";
import { Observable } from "rxjs";
import { BaseDALService } from "./base-dal.service";
import { AdminDashboardStatsDTO } from "../2_Models/responses/admin-dashboard-stats.model";
import { AdminUserDTO } from "../2_Models/responses/admin-user.model";
import { AdminTransactionDTO } from "../2_Models/responses/admin-transaction.model";

@Injectable({
    providedIn: 'root'
})
export class AdminDALService extends BaseDALService {
    override readonly controller = 'admin';

    getDashboardStats$(): Observable<BaseResult<AdminDashboardStatsDTO>> {
        return this.SendGet$('dashboard-stats');
    }

    getUsers$(search?: string, role?: string, enabled?: boolean): Observable<BaseResult<AdminUserDTO[]>> {
        const params = new URLSearchParams();
        if (search && search.trim() !== '') {
            params.set('search', search.trim());
        }
        if (role && role.trim() !== '' && role !== 'all') {
            params.set('role', role.trim());
        }
        if (enabled !== undefined && enabled !== null) {
            params.set('enabled', String(enabled));
        }
        const query = params.toString();
        return this.SendGet$(query ? `users?${query}` : 'users');
    }

    toggleUserStatus$(userId: number): Observable<BaseResult<SaveResponse>> {
        return this.SendPost$(`users/${userId}/toggle`, {});
    }

    getTransactions$(search?: string, type?: string): Observable<BaseResult<AdminTransactionDTO[]>> {
        const params = new URLSearchParams();
        if (search && search.trim() !== '') {
            params.set('search', search.trim());
        }
        if (type && type.trim() !== '' && type !== 'all') {
            params.set('type', type.trim());
        }
        const query = params.toString();
        return this.SendGet$(query ? `transactions?${query}` : 'transactions');
    }
}
