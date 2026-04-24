import { Routes } from '@angular/router';
import { HomeComponent } from './www/5_Modules/home/home-component/home-component';
import { NavConstants } from './www/6_Common/nav-constants';
import { LoginComponent } from './www/5_Modules/user/login-component/login-component';
import { LogoutComponent } from './www/5_Modules/user/logout-component/logout-component';

export const routes: Routes = [
    {
        path: "",
        component: HomeComponent
    },
    {
        path: NavConstants.login,
        component: LoginComponent
    },
    {
        path: NavConstants.signup,
        component: LoginComponent
    },
    {
        path: NavConstants.logout,
        component: LogoutComponent
    }
];
