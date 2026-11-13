import { inject, Injectable } from "@angular/core";
import { map, Observable } from "rxjs";
import { BaseResult } from "../2_Models/common/base-result.model";
import { ResultType } from "../2_Models/common/result-type.model";
import { BaseBLLService } from "./base-bll.service";
import { AdminDALService } from "../3_DAL/admin-dal.service";
import { SaveResponse } from "../2_Models/common/save-response.model";
import { AdminDashboardStatsDTO } from "../2_Models/responses/admin-dashboard-stats.model";
import { AdminUserDTO } from "../2_Models/responses/admin-user.model";
import { AdminTransactionDTO } from "../2_Models/responses/admin-transaction.model";

@Injectable({
    providedIn: 'root'
})
export class AdminBLLService extends BaseBLLService {
    private adminDALService = inject(AdminDALService);

    getDashboardStats$(): Observable<AdminDashboardStatsDTO | null> {
        return this.unwrapResult(this.adminDALService.getDashboardStats$());
    }

    getUsers$(search?: string, role?: string, enabled?: boolean): Observable<AdminUserDTO[] | null> {
        return this.unwrapResult(this.adminDALService.getUsers$(search, role, enabled));
    }

    toggleUserStatus$(userId: number): Observable<SaveResponse | null> {
        return this.unwrapResult(this.adminDALService.toggleUserStatus$(userId));
    }

    getTransactions$(search?: string, type?: string): Observable<AdminTransactionDTO[] | null> {
        return this.unwrapResult(this.adminDALService.getTransactions$(search, type));
    }

    private unwrapResult<T>(request$: Observable<BaseResult<T>>): Observable<T | null> {
        return request$.pipe(
            map((response: BaseResult<T>) => {
                if (response && response.resultType === ResultType.Success && response.model !== undefined && response.model !== null) {
                    return response.model;
                }
                if (response && response.resultType !== ResultType.Success) {
                    this.handleResult(response);
                }
                return null;
            })
        );
    }
}
