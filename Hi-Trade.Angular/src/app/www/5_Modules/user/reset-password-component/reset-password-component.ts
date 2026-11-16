import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { UserBLLService } from '../../../4_BLL/user-bll.service';
import { NotificationService } from '../../../1_Services/notification.service';
import { NavConstants } from '../../../6_Common/nav-constants';
import { NavBarComponent } from '../../shared/nav-bar-component/nav-bar-component';

@Component({
  selector: 'app-reset-password-component',
  standalone: true,
  imports: [CommonModule, FormsModule, NavBarComponent],
  templateUrl: './reset-password-component.html',
  styleUrl: './reset-password-component.css'
})
export class ResetPasswordComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private userBLLService = inject(UserBLLService);
  private notificationService = inject(NotificationService);

  email: string = '';
  token: string = '';
  newPassword: string = '';
  confirmPassword: string = '';
  errorMessage = signal('');
  isLoading = signal(false);
  navConstants = NavConstants;

  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      if (params['email']) {
        this.email = params['email'];
      }
      if (params['token']) {
        this.token = params['token'];
      }
      if (!this.token) {
        this.errorMessage.set('Invalid or missing password reset token. Please request a new link.');
      }
    });
  }

  onSubmit(): void {
    this.errorMessage.set('');

    if (!this.email || !this.token) {
      this.errorMessage.set('Missing email or reset token. Please request a new link.');
      return;
    }

    if (!this.newPassword || this.newPassword.length < 12) {
      this.errorMessage.set('Password must be at least 12 characters long.');
      return;
    }

    if (this.newPassword !== this.confirmPassword) {
      this.errorMessage.set('Passwords do not match.');
      return;
    }

    this.isLoading.set(true);

    this.userBLLService.resetPassword$({
      email: this.email,
      token: this.token,
      newPassword: this.newPassword
    }).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        if (response?.success) {
          this.notificationService.showSuccessToast('Password reset successfully! Please sign in.');
          this.router.navigate([this.navConstants.login]);
        } else {
          this.errorMessage.set(response?.message ?? 'Failed to reset password.');
        }
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorMessage.set('An error occurred while resetting password. Please try again.');
      }
    });
  }

  redirectToLogin(): void {
    this.router.navigate([this.navConstants.login]);
  }
}
