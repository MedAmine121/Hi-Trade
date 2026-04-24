import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { AuthService } from '../../../1_Services/auth.service';
import { UserIndexComponent } from "../user-index-component/user-index-component";
import { LandingPageComponent } from '../../landing-page/landing-page.component';

@Component({
  selector: 'app-home-component',
  imports: [UserIndexComponent, LandingPageComponent],
  templateUrl: './home-component.html',
  styleUrl: './home-component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HomeComponent implements OnInit {
  isAuthenticated = 0;
  private authService = inject(AuthService);
  ngOnInit(): void {
    this.isAuthenticated += this.authService.isAuthenticated() ? 1:0;
    this.isAuthenticated += this.authService.isAdmin() ? 1:0;
  }
}
