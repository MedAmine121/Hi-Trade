import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { UserBLLService } from '../../../4_BLL/user-bll.service';
import { NotificationService } from '../../../1_Services/notification.service';
import { StorageService } from '../../../1_Services/storage.service';
import { UserService } from '../../../1_Services/user.service';
import { Constants } from '../../../6_Common/constants';
import { Context } from '../../../2_Models/responses/context.model';
import { NavConstants } from '../../../6_Common/nav-constants';
import { NavBarComponent } from '../../shared/nav-bar-component/nav-bar-component';

@Component({
  selector: 'app-verify-email-component',
  standalone: true,
  imports: [CommonModule, FormsModule, NavBarComponent],
  templateUrl: './verify-email-component.html',
  styleUrl: './verify-email-component.css'
})
export class VerifyEmailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private userBLLService = inject(UserBLLService);
  private notificationService = inject(NotificationService);
  private storageService = inject(StorageService);
  private userService = inject(UserService);

  email: string = '';
  code: string = '';
  isLoading = signal(false);
  isSuccess = signal(false);
  errorMessage = signal('');
  isResending = signal(false);
  resendSuccess = signal('');
  navConstants = NavConstants;

  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      if (params['email']) {
        this.email = params['email'];
      } else {
        const ctx = this.storageService.getLocalStorage(Constants.CONTEXT_KEY) as Context | null;
        if (ctx?.email) {
          this.email = ctx.email;
        }
      }
      if (params['code']) {
        this.code = params['code'];
        this.submitVerification();
      }
    });
  }

  submitVerification(): void {
    const userEmail = this.email.trim();
    const verificationCode = this.code.trim();

    if (!userEmail || !verificationCode) {
      this.errorMessage.set('Please provide both email and 6-digit verification code.');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set('');

    this.userBLLService.verifyEmail$({ email: userEmail, code: verificationCode }).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        if (response?.success) {
          this.isSuccess.set(true);
          this.notificationService.showSuccessToast('Email verified successfully!');
          // Update context
          const ctx = this.storageService.getLocalStorage(Constants.CONTEXT_KEY) as Context | null;
          if (ctx) {
            ctx.isEmailVerified = true;
            this.storageService.setLocalStorage(Constants.CONTEXT_KEY, ctx);
            this.userService.user.set(ctx);
          }
          setTimeout(() => {
            this.router.navigate([this.navConstants.home]);
          }, 1500);
        } else {
          this.errorMessage.set(response?.message ?? 'Verification failed.');
        }
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorMessage.set('An error occurred during verification.');
      }
    });
  }

  resendCode(): void {
    if (!this.email) {
      this.errorMessage.set('Email is missing.');
      return;
    }

    this.isResending.set(true);
    this.resendSuccess.set('');
    this.errorMessage.set('');

    this.userBLLService.resendVerification$({ email: this.email }).subscribe({
      next: (response) => {
        this.isResending.set(false);
        if (response?.success) {
          this.resendSuccess.set('A new code has been sent to your email.');
          this.notificationService.showSuccessToast('Verification code resent.');
        } else {
          this.errorMessage.set(response?.message ?? 'Failed to resend code.');
        }
      },
      error: (err) => {
        this.isResending.set(false);
        this.errorMessage.set('Failed to resend verification code.');
      }
    });
  }

  redirectToHome(): void {
    this.router.navigate([this.navConstants.home]);
  }
}
