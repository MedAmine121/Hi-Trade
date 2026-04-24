import { Component, inject } from '@angular/core';
import { NotificationService } from '../../../1_Services/notification.service';
import { StorageService } from '../../../1_Services/storage.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-base-component',
  imports: [],
  templateUrl: './base-component.html',
  styleUrl: './base-component.css',
})
export class BaseComponent {
  protected notificationService = inject(NotificationService);
  protected storageService = inject(StorageService);
  protected router = inject(Router);
}
