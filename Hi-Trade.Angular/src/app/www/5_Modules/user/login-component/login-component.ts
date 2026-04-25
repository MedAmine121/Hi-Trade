import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { UserBLLService } from '../../../4_BLL/user-bll.service';
import { LoginUserRequest } from '../../../2_Models/requests/login-request.model';
import { Context } from '../../../2_Models/responses/context.model';
import { BaseComponent } from '../../shared/base-component/base-component';
import { Constants } from '../../../6_Common/constants';
import { NavBarComponent } from "../../shared/nav-bar-component/nav-bar-component";

@Component({
  selector: 'app-login-component',
  imports: [FormsModule, NavBarComponent],
  templateUrl: './login-component.html',
  styleUrl: './login-component.css',
})
export class LoginComponent extends BaseComponent{
  email: string = '';
  password: string = '';
  rememberMe: boolean = false;
  private userService = inject(UserBLLService);

  onLogin(): void {
    if (this.email && this.password) {
      const request = <LoginUserRequest>{
        email: this.email,
        password: this.password
      }
      this.userService.login(request).subscribe({
        next: (response: Context | null) => {
          if(response !== null){
            this.notificationService.showSuccessToast('Login Successful');
            this.storageService.setLocalStorage(Constants.CONTEXT_KEY,response);
            this.router.navigate([this.navConstants.home]);
          }
        },
        error: (err: Error) => {
          this.notificationService.showErrorToast('An error has occured, please try again later.');
        }
      })
    }
  }
}
