import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  inject,
  output,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { AssetBLLService } from '../../../4_BLL/asset-bll.service';
import { BaseComponent } from '../base-component/base-component';

@Component({
  selector: 'app-create-asset-modal-component',
  imports: [FormsModule],
  templateUrl: './create-asset-modal-component.html',
  styleUrl: './create-asset-modal-component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CreateAssetModalComponent extends BaseComponent {
  private assetBLLService = inject(AssetBLLService);
  private destroyRef = inject(DestroyRef);

  assetCreated = output<void>();

  isOpen = signal(false);
  isSubmitting = signal(false);
  ticker = signal('');
  name = signal('');
  validationError = signal('');

  openModal(): void {
    this.ticker.set('');
    this.name.set('');
    this.validationError.set('');
    this.isOpen.set(true);
  }

  closeModal(): void {
    if (this.isSubmitting()) {
      return;
    }
    this.validationError.set('');
    this.isOpen.set(false);
  }

  onTickerInput(value: string): void {
    this.ticker.set(value.toUpperCase());
    if (this.validationError()) {
      this.validationError.set('');
    }
  }

  onNameInput(value: string): void {
    this.name.set(value);
    if (this.validationError()) {
      this.validationError.set('');
    }
  }

  onSubmit(): void {
    const cleanTicker = this.ticker().trim().toUpperCase();
    const cleanName = this.name().trim();

    if (!cleanTicker || !cleanName) {
      this.validationError.set('Both ticker symbol and asset name are required.');
      return;
    }

    this.validationError.set('');
    this.isSubmitting.set(true);

    this.assetBLLService
      .createAsset$({ ticker: cleanTicker, name: cleanName })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isSubmitting.set(false)),
      )
      .subscribe({
        next: (response) => {
          if (!response) {
            return;
          }
          if (response.success) {
            this.notificationService.showSuccessToast(
              response.message || 'Asset created successfully.',
            );
            this.isOpen.set(false);
            this.assetCreated.emit();
          } else {
            this.validationError.set(response.message || 'Failed to create asset.');
            this.notificationService.showErrorToast(
              response.message || 'Failed to create asset.',
            );
          }
        },
        error: () => {
          this.validationError.set('An unexpected error occurred while creating the asset.');
        },
      });
  }
}
