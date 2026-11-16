import { Routes } from '@angular/router';
import { NavConstants } from './www/6_Common/nav-constants';
import { adminGuard } from './www/1_Services/admin.guard';

export const routes: Routes = [
    {
        path: "",
        loadComponent: () => import('./www/5_Modules/home/home-component/home-component').then((m) => m.HomeComponent)
    },
    {
        path: NavConstants.admin,
        canActivate: [adminGuard],
        loadComponent: () => import('./www/5_Modules/home/admin-index-component/admin-index-component').then((m) => m.AdminIndexComponent)
    },
    {
        path: NavConstants.backoffice,
        canActivate: [adminGuard],
        loadComponent: () => import('./www/5_Modules/home/admin-index-component/admin-index-component').then((m) => m.AdminIndexComponent)
    },
    {
        path: NavConstants.login,
        loadComponent: () => import('./www/5_Modules/user/login-component/login-component').then((m) => m.LoginComponent)
    },
    {
        path: NavConstants.signup,
        loadComponent: () => import('./www/5_Modules/user/signup-component/signup-component').then((m) => m.SignupComponent)
    },
    {
        path: NavConstants.logout,
        loadComponent: () => import('./www/5_Modules/user/logout-component/logout-component').then((m) => m.LogoutComponent)
    },
    {
        path: NavConstants.resetPassword,
        loadComponent: () => import('./www/5_Modules/user/reset-password-component/reset-password-component').then((m) => m.ResetPasswordComponent)
    },
    {
        path: NavConstants.verifyEmail,
        loadComponent: () => import('./www/5_Modules/user/verify-email-component/verify-email-component').then((m) => m.VerifyEmailComponent)
    }
];
