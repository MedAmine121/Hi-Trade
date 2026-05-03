import { Component, EventEmitter, input, Input, Output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AssetDTO } from '../../../2_Models/responses/asset.model';

@Component({
  selector: 'app-buy-asset-modal-component',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './buy-asset-modal-component.html',
  styleUrl: './buy-asset-modal-component.css',
})
export class BuyAssetModalComponent {
  assets = input.required<AssetDTO[]>();
  @Output() buy = new EventEmitter<{ asset: AssetDTO; quantity: number }>();
  @Output() cancel = new EventEmitter<void>();

  isOpen = signal(false);
  selectedAsset: AssetDTO | null = null;
  quantity = signal(1);

  openModal(): void {
    this.isOpen.set(true);
    this.selectedAsset = null;
    this.quantity.set(1);
  }

  closeModal(): void {
    this.isOpen.set(false);
    this.selectedAsset = null;
    this.quantity.set(1);
    this.cancel.emit();
  }

  selectAsset(asset: AssetDTO): void {
    this.selectedAsset = asset;
  }

  onBuy(): void {
    if (this.selectedAsset && this.quantity() > 0) {
      this.buy.emit({
        asset: this.selectedAsset,
        quantity: this.quantity()
      });
      this.closeModal();
    }
  }

  onCancel(): void {
    this.closeModal();
  }

  isAssetSelected(asset: AssetDTO): boolean {
    return this.selectedAsset?.ticker === asset.ticker;
  }
}
