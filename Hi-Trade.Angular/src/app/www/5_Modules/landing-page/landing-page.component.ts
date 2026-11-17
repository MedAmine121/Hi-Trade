import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { Router } from '@angular/router';
import { NavBarComponent } from '../shared/nav-bar-component/nav-bar-component';
import { NavConstants } from '../../6_Common/nav-constants';

@Component({
  selector: 'app-landing-page',
  imports: [NavBarComponent],
  templateUrl: './landing-page.component.html',
  styleUrl: './landing-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LandingPageComponent {
  private router = inject(Router);

  navigateToSignup(): void {
    this.router.navigate([NavConstants.signup]);
  }

  navigateToLogin(): void {
    this.router.navigate([NavConstants.login]);
  }
}
