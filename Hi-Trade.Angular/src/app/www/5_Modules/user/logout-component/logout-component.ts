import { Component, inject, OnInit } from '@angular/core';
import { BaseComponent } from '../../shared/base-component/base-component';
import { UserBLLService } from '../../../4_BLL/user-bll.service';
import { SaveResponse } from '../../../2_Models/common/save-response.model';
import { Constants } from '../../../6_Common/constants';

@Component({
  selector: 'app-logout-component',
  imports: [],
  templateUrl: './logout-component.html',
  styleUrl: './logout-component.css',
})
export class LogoutComponent extends BaseComponent implements OnInit {
  private userService = inject(UserBLLService);
  ngOnInit(): void {
    this.userService.logout().subscribe({
      next: (response: SaveResponse | null) => {
        if(response !== null && response.success){
          this.notificationService.showSuccessToast('Logout Successful');
          this.storageService.removeLocalStorage(Constants.CONTEXT_KEY);
          this.router.navigate(['']);
        }
        else if(!response?.success){
          this.notificationService.showErrorToast(response?.message ?? '');
        }
      },
      error: (err: Error) => {
        this.notificationService.showErrorToast('An error has occured, please try again later.');
      }
    })
  }
}
