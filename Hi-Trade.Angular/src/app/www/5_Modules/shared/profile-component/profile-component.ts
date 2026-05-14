import { Component, EventEmitter, inject, Output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BaseComponent } from '../base-component/base-component';
import { UserService } from '../../../1_Services/user.service';
import { Context } from '../../../2_Models/responses/context.model';
import { UserBLLService } from '../../../4_BLL/user-bll.service';
import { Constants } from '../../../6_Common/constants';

@Component({
  selector: 'app-profile-component',
  imports: [FormsModule],
  templateUrl: './profile-component.html',
  styleUrl: './profile-component.css',
})
export class ProfileComponent extends BaseComponent {
  protected userService = inject(UserService);
  private userBLLService = inject(UserBLLService);
  user = this.userService.user;
  profilePicSignal = signal('');
  @Output() cancel = new EventEmitter<void>();

  isOpen = signal(false);
  isEditMode = signal(false);
  formData = {
    fullName: '',
    email: '',
    address: '',
    profilePictureUrl: '',
  };

  openModal(): void {
    this.userService.fetchUser();
    this.isOpen.set(true);
    this.isEditMode.set(false);
    this.profilePicSignal.set('');
  }

  closeModal(): void {
    this.isOpen.set(false);
    this.isEditMode.set(false);
    this.cancel.emit();
  }

  onCancel(): void {
    this.closeModal();
  }

  onEdit(): void {
    // Load current user data into form
    const currentUser = this.user();
    if (currentUser) {
      this.formData = {
        fullName: currentUser.fullName,
        email: currentUser.email,
        address: currentUser.address,
        profilePictureUrl: '',
      };
      this.isEditMode.set(true);
    }
  }

  onCancelEdit(): void {
    this.isEditMode.set(false);
    this.profilePicSignal.set('');
    // Reset form data
    const currentUser = this.user();
    if (currentUser) {
      this.formData = {
        fullName: currentUser.fullName,
        email: currentUser.email,
        address: currentUser.address,
        profilePictureUrl: '',
      };
    }
  }

  onSave(): void {
    if (this.isEditMode()) {
      const currentUser = this.user();
      if (currentUser) {
        this.formData.profilePictureUrl = this.profilePicSignal() || currentUser.profilePictureUrl;
        this.userBLLService.editProfile$(this.formData).subscribe({
          next: (result: Context | null) => {
            if(result){
              this.notificationService.showSuccessToast('Profile updated successfully');
              this.storageService.setLocalStorage(Constants.CONTEXT_KEY,result);
              this.userService.fetchUser();
              this.isEditMode.set(false);
            }
          },
          error: (err) => {
            this.notificationService.showErrorToast(err.message || 'An error occurred while updating profile');
          }
        });
      }
      this.isEditMode.set(false);
      this.profilePicSignal.set('');
    } else {
      this.closeModal();
    }
  }

  getAvatarInitials(name: string): string {
    return this.userService.getAvatarInitials(name);
  }

  onProfileImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    
    if (file) {
      const reader = new FileReader();
      reader.onload = (e) => {
        this.profilePicSignal.set(e.target?.result as string);
      };
      reader.readAsDataURL(file);
    }
  }
}
