import { Component, EventEmitter, input, Output, signal } from '@angular/core';
import { TransactionDTO } from '../../../2_Models/responses/transaction.model';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-transaction-history-component',
  imports: [CommonModule],
  templateUrl: './transaction-history-component.html',
  styleUrl: './transaction-history-component.css',
})
export class TransactionHistoryComponent {
  transactions = input.required<TransactionDTO[]>();
  @Output() cancel = new EventEmitter<void>();

  isOpen = signal(false);
  quantity = signal(1);

  openModal(): void {
    this.isOpen.set(true);
    this.quantity.set(1);
  }

  closeModal(): void {
    this.isOpen.set(false);
    this.quantity.set(1);
    this.cancel.emit();
  }

  

  onCancel(): void {
    this.closeModal();
  }
}
