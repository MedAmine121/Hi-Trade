import { inject } from "@angular/core";
import { BaseResult } from "../2_Models/common/base-result.model";
import { ResultType } from "../2_Models/common/result-type.model";
import { NotificationService } from "../1_Services/notification.service";

export class BaseBLLService {
    private notificationService = inject(NotificationService);
    handleResult(response: BaseResult<any>): void{
        if(response.resultType === ResultType.Error){
            this.notificationService.showErrorToast('An error has occured. Please try again later.');
        }
        else if(response.resultType === ResultType.Fail){
            this.notificationService.showErrorToast(response?.message ?? '');
        }
        else if(response.resultType === ResultType.BadRequest){
            let message = response.message ?? '';
            message = message.replace("Validation failed: \r\n -- ","");
            message = message.replace("Severity: Error","");
            this.notificationService.showErrorToast(message);
        }
    }
}