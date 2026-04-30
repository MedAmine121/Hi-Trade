import { Component, inject, OnInit, HostListener } from '@angular/core';

import { Router } from '@angular/router';
import { BaseComponent } from '../base-component/base-component';
import { AuthService } from '../../../1_Services/auth.service';
import { NavConstants } from '../../../6_Common/nav-constants';

@Component({
  selector: 'app-nav-bar-component',
  imports: [],
  templateUrl: './nav-bar-component.html',
  styleUrl: './nav-bar-component.css',
})
export class NavBarComponent extends BaseComponent implements OnInit {
  private authService = inject(AuthService);
  isAuthenticated: boolean = false;
  userName: string = '';
  userAvatar: string = '👤';
  showProfileDropdown: boolean = false;
  showInitials = false;

  ngOnInit(): void {
    this.isAuthenticated = this.authService.isAuthenticated();
    if (this.isAuthenticated) {
      const context = this.authService.getContext();
      this.userName = context.fullName;
      if (!context.profilePictureUrl) {
        this.showInitials = true;
        this.userAvatar = this.getAvatarInitials(this.userName);
      } else {
        this.userAvatar = context.profilePictureUrl;
      }
    }
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    const profileMenu = document.querySelector('.profile-menu');
    if (profileMenu && !profileMenu.contains(target)) {
      this.closeProfileDropdown();
    }
  }

  toggleProfileDropdown(): void {
    this.showProfileDropdown = !this.showProfileDropdown;
  }

  closeProfileDropdown(): void {
    this.showProfileDropdown = false;
  }

  getAvatarInitials(name: string): string {
    const initials = name
      .split(' ')
      .map((n) => n.charAt(0).toUpperCase())
      .join('');
    return initials || '👤';
  }

  onLogout(): void {
    this.redirectTo([this.navConstants.logout]);
  }
}
