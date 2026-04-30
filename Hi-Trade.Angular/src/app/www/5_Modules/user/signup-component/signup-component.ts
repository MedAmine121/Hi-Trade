import { Component, inject, OnInit } from '@angular/core';
import { ReactiveFormsModule, FormGroup, FormControl, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { NavBarComponent } from "../../shared/nav-bar-component/nav-bar-component";
import { BaseComponent } from '../../shared/base-component/base-component';
import { UserBLLService } from '../../../4_BLL/user-bll.service';
import { CreateUserRequest } from '../../../2_Models/requests/create-user-request.model';
import { Context } from '../../../2_Models/responses/context.model';
import { Constants } from '../../../6_Common/constants';

@Component({
    selector: 'app-signup-component',
    imports: [ReactiveFormsModule, NavBarComponent],
    templateUrl: './signup-component.html',
    styleUrl: './signup-component.css',
})
export class SignupComponent extends BaseComponent implements OnInit {
    signupForm!: FormGroup;
    private userService = inject(UserBLLService);

    ngOnInit(): void {
        this.signupForm = new FormGroup(
            {
                fullname: new FormControl('', [Validators.required]),
                email: new FormControl('', [Validators.required, Validators.email]),
                address: new FormControl('', [Validators.required]),
                password: new FormControl('', [Validators.required, Validators.minLength(12)]),
                confirmPassword: new FormControl('', [Validators.required]),
                agreedToTerms: new FormControl(false, [Validators.requiredTrue]),
            },
            { validators: this.passwordMatchValidator }
        );
    }

    passwordMatchValidator(group: AbstractControl): ValidationErrors | null {
        const password = group.get('password')?.value;
        const confirmPassword = group.get('confirmPassword')?.value;
        return password === confirmPassword ? null : { passwordMismatch: true };
    }

    onSignup(): void {
        if (this.signupForm.invalid) {
            this.notificationService.showErrorToast('Please fill in all fields correctly');
            return;
        }

        const formData = this.signupForm.value;
        const request = <CreateUserRequest>{
            email: formData.email,
            password: formData.password,
            fullName: formData.fullname,
            address: formData.address
        };
        this.userService.signup$(request).subscribe({
            next: (response: Context | null) => {
                if (response !== null) {
                    this.notificationService.showSuccessToast('Signup Successful');
                    this.storageService.setLocalStorage(Constants.CONTEXT_KEY, response);
                    this.router.navigate([this.navConstants.home]);
                }
            },
            error: (err: Error) => {
                this.notificationService.showErrorToast('An error has occured, please try again later.');
            }
        })
    }
}

