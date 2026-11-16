import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserBLLService } from '../../../4_BLL/user-bll.service';
import { NotificationService } from '../../../1_Services/notification.service';

@Component({
  selector: 'app-forgot-password-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './forgot-password-modal.component.html',
  styleUrl: './forgot-password-modal.component.css'
})
export class ForgotPasswordModalComponent {
  private userBLLService = inject(UserBLLService);
  private notificationService = inject(NotificationService);

  isOpen = signal(false);
  email = signal('');
  isLoading = signal(false);
  statusMessage = signal('');
  isSuccess = signal(false);

  openModal(prefillEmail?: string): void {
    this.email.set(prefillEmail ?? '');
    this.statusMessage.set('');
    this.isSuccess.set(false);
    this.isOpen.set(true);
  }

  closeModal(): void {
    this.isOpen.set(false);
  }

  onSubmit(): void {
    const userEmail = this.email().trim();
    if (!userEmail) {
      this.statusMessage.set('Please enter your email address.');
      this.isSuccess.set(false);
      return;
    }

    this.isLoading.set(true);
    this.statusMessage.set('');

    this.userBLLService.forgotPassword$({ email: userEmail }).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        this.isSuccess.set(true);
        this.statusMessage.set(
          response?.message ?? 'If an account with that email exists, password reset instructions have been sent.'
        );
        this.notificationService.showSuccessToast('Password reset link sent.');
      },
      error: (err) => {
        this.isLoading.set(false);
        this.isSuccess.set(false);
        this.statusMessage.set('An error occurred. Please try again later.');
      }
    });
  }
}
