import { Component, EventEmitter, Input, Output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PositionDTO } from '../../../2_Models/responses/position.model';

@Component({
  selector: 'app-sell-asset-modal-component',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './sell-asset-modal-component.html',
  styleUrl: './sell-asset-modal-component.css',
})
export class SellAssetModalComponent {
  @Input() positions: PositionDTO[] = [];
  @Output() sell = new EventEmitter<{ position: PositionDTO; quantity: number }>();
  @Output() cancel = new EventEmitter<void>();

  isOpen = signal(false);
  selectedPosition: PositionDTO | null = null;
  quantity = signal(1);

  openModal(position: PositionDTO): void {
    this.isOpen.set(true);
    this.selectedPosition = position;
    this.quantity.set(1);
  }

  closeModal(): void {
    this.isOpen.set(false);
    this.selectedPosition = null;
    this.quantity.set(1);
    this.cancel.emit();
  }

  onSell(): void {
    if (this.selectedPosition && this.quantity() > 0 && this.quantity() <= this.selectedPosition.quantity) {
      this.sell.emit({
        position: this.selectedPosition,
        quantity: this.quantity()
      });
      this.closeModal();
    }
  }

  onCancel(): void {
    this.closeModal();
  }

  getPositionValue(quantity: number, price: number): number {
    return quantity * price;
  }

  isQuantityValid(): boolean {
    return this.quantity() > 0 && this.quantity() <= (this.selectedPosition?.quantity || 0);
  }
}
