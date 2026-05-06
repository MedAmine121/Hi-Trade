import { Component, EventEmitter, inject, Output, signal, ViewChild } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { UserBLLService } from '../../../4_BLL/user-bll.service';
import { AddFundsRequest } from '../../../2_Models/requests/add-funds-request.model';
import { NotificationService } from '../../../1_Services/notification.service';
import { PaymentIntentResult, StripeElementsOptions } from '@stripe/stripe-js';
import { SaveResponse } from '../../../2_Models/common/save-response.model';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatDividerModule } from '@angular/material/divider';
import { MatInputModule } from '@angular/material/input';
import { MatToolbarModule } from '@angular/material/toolbar';
import { injectStripe, NgxStripeModule, StripePaymentElementComponent } from 'ngx-stripe';
import { CurrencyPipe } from '@angular/common';
import { environment } from '../../../../../environments/environment';
import { UserService } from '../../../1_Services/user.service';
@Component({
  selector: 'app-add-funds-component',
  imports: [FormsModule, MatButtonModule,
    MatCardModule,
    MatDialogModule,
    MatDividerModule,
    MatInputModule,
    MatToolbarModule,
    NgxStripeModule,
  CurrencyPipe,
    ReactiveFormsModule],
  templateUrl: './add-funds-component.html',
  styleUrl: './add-funds-component.css',
})
export class AddFundsComponent {
  isOpen = signal(false);
  amount = signal(1);
  proceeded = signal(false);
  paying = signal(false);
  elementsOptions: StripeElementsOptions = {
    locale: 'en',
    appearance: {
      theme: 'stripe',
      labels: 'floating',
      variables: {
        colorPrimary: '#673ab7',
      },
    },
  };
  @Output() cancel = new EventEmitter<void>();
  @ViewChild(StripePaymentElementComponent)
  paymentElement!: StripePaymentElementComponent;
  private readonly dialog = inject(MatDialog);
  private userBLLService = inject(UserBLLService);
  private userService = inject(UserService);
  private notificationService = inject(NotificationService);
  private readonly fb = inject(FormBuilder);
  readonly stripe = injectStripe(environment.stripePublicKey);
  checkoutForm = this.fb.group({
    name: ['', [Validators.required]],
    email: ['', [Validators.required]],
    address: ['', [Validators.required]],
    zipcode: ['', [Validators.required]],
    city: ['', [Validators.required]],
    amount: [this.amount(), [Validators.required, Validators.pattern(/\d+/)]],
  });
  openModal(): void {
    this.isOpen.set(true);
    this.amount.set(1);
    console.log(environment.stripePublicKey);
  }
  closeModal(): void {
    this.isOpen.set(false);
    this.amount.set(1);
    this.proceeded.set(false); 
    this.cancel.emit();
  }
  onCancel(): void {
    this.closeModal();
  }
  onProceed(): void{
    const request = <AddFundsRequest>{
      amount: this.amount()
    }
    this.userBLLService.checkout$(request).subscribe({next: (response: SaveResponse | null) => {
      if(response?.success){
        this.elementsOptions.clientSecret = response.message;
        this.proceeded.set(true);
      }
    },
      error: (err: Error) => {
        this.notificationService.showErrorToast('Failed to add funds. Please try again.');
      }
    });
  }
  
  clear() {
    this.checkoutForm.patchValue({
      name: '',
      email: '',
      address: '',
      zipcode: '',
      city: '',
      amount: 100,
    });
  }
  collectPayment() {
    if (this.paying() || this.checkoutForm.invalid) return;
    this.paying.set(true);

    const { name, email, address, zipcode, city } =
      this.checkoutForm.getRawValue();

    this.stripe
      .confirmPayment({
        elements: this.paymentElement.elements,
        confirmParams: {
          payment_method_data: {
            billing_details: {
              name: name as string,
              email: email as string,
              address: {
                line1: address as string,
                postal_code: zipcode as string,
                city: city as string,
              },
            },
          },
        },
        redirect: 'if_required',
      })
      .subscribe({
        next: (result: PaymentIntentResult) => {
          this.paying.set(false);
          if (result.error) {
            this.notificationService.showErrorToast(result.error.message || 'Payment failed. Please try again.');
          } else if (result.paymentIntent.status === 'succeeded') {
            this.userBLLService.confirmPayment$({ paymentId: result.paymentIntent.id }).subscribe({
              next: (response: SaveResponse | null) => {
                if(response?.success){
                  this.closeModal();
                  this.notificationService.showSuccessToast('Funds added successfully!');
                  this.userService.fetchUser();
                } else {
                  this.notificationService.showErrorToast('Payment succeeded but failed to confirm. Please contact support.');
                }
              },
              error: (err: Error) => {
                this.notificationService.showErrorToast('An error occurred while confirming payment. Please contact support.');
              }
            });
          }
        },
        error: (err: Error) => {
          this.paying.set(false);
          this.notificationService.showErrorToast('An error occurred. Please try again.');
        },
      });
  }
}
