import { Component, EventEmitter, Output, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserBLLService } from '../../../4_BLL/user-bll.service';
import { NotificationService } from '../../../1_Services/notification.service';
import { StorageService } from '../../../1_Services/storage.service';
import { Constants } from '../../../6_Common/constants';
import { Context } from '../../../2_Models/responses/context.model';

@Component({
  selector: 'app-verify-email-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './verify-email-modal.component.html',
  styleUrl: './verify-email-modal.component.css'
})
export class VerifyEmailModalComponent {
  @Output() verified = new EventEmitter<void>();

  private userBLLService = inject(UserBLLService);
  private notificationService = inject(NotificationService);
  private storageService = inject(StorageService);

  isOpen = signal(false);
  email = signal('');
  verificationCode = signal('');
  codeSent = signal(false);
  isLoading = signal(false);
  isResending = signal(false);
  resendSuccess = signal('');
  errorMessage = signal('');

  openModal(email?: string, codeAlreadySent: boolean = false): void {
    if (email) {
      this.email.set(email);
    } else {
      const context = this.storageService.getLocalStorage(Constants.CONTEXT_KEY) as Context | null;
      if (context?.email) {
        this.email.set(context.email);
      }
    }
    this.verificationCode.set('');
    this.errorMessage.set('');
    this.resendSuccess.set('');
    this.codeSent.set(codeAlreadySent);
    this.isOpen.set(true);
  }

  closeModal(): void {
    this.isOpen.set(false);
  }

  showCodeInput(): void {
    this.errorMessage.set('');
    this.codeSent.set(true);
  }

  sendVerificationCode(): void {
    this.resendCode();
  }

  verify(): void {
    const code = this.verificationCode().trim();
    const userEmail = this.email().trim();

    if (!code) {
      this.errorMessage.set('Please enter the 6-digit verification code.');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set('');
    this.resendSuccess.set('');

    this.userBLLService.verifyEmail$({ email: userEmail, code }).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        if (response?.success) {
          this.isOpen.set(false);
          this.verified.emit();
        } else {
          this.errorMessage.set(response?.message ?? 'Verification failed. Please try again.');
        }
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorMessage.set('An error occurred during email verification.');
      }
    });
  }

  resendCode(): void {
    const userEmail = this.email().trim();
    if (!userEmail) {
      this.errorMessage.set('Email address is missing.');
      return;
    }

    this.isResending.set(true);
    this.errorMessage.set('');
    this.resendSuccess.set('');

    this.userBLLService.resendVerification$({ email: userEmail }).subscribe({
      next: (response) => {
        this.isResending.set(false);
        if (response?.success) {
          this.codeSent.set(true);
          this.resendSuccess.set('A new verification code has been sent to your email.');
          this.notificationService.showSuccessToast('Verification code sent successfully.');
        } else {
          this.errorMessage.set(response?.message ?? 'Failed to send code.');
        }
      },
      error: (err) => {
        this.isResending.set(false);
        this.errorMessage.set('Failed to send verification code. Please try again.');
      }
    });
  }
}
