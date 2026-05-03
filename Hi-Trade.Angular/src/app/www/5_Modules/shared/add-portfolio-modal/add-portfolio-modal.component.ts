import { Component, EventEmitter, Output, Signal, signal } from '@angular/core';

import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-add-portfolio-modal',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './add-portfolio-modal.component.html',
  styleUrl: './add-portfolio-modal.component.css',
})
export class AddPortfolioModalComponent {
  @Output() submit = new EventEmitter<string>();
  @Output() cancel = new EventEmitter<void>();

  portfolioName = signal('');
  isOpen = signal(false);

  openModal(): void {
    this.isOpen.set(true);
    this.portfolioName.set('');
  }

  closeModal(): void {
    this.isOpen.set(false);
    this.portfolioName.set('');
    this.cancel.emit();
  }

  onSubmit(): void {
    const name = this.portfolioName().trim();
    if (name) {
      this.submit.emit(name);
      this.closeModal();
    }
  }

  onCancel(): void {
    this.closeModal();
  }
}
