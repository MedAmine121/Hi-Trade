import { Inject, inject, Injectable, signal } from "@angular/core";
import { Context } from "../2_Models/responses/context.model";
import { UserBLLService } from "../4_BLL/user-bll.service";
import { NotificationService } from "./notification.service";
@Injectable({
    providedIn: 'root'
})
export class UserService {
    user = signal<Context | null>(null);
    private userBLL = inject(UserBLLService);
    private notificationService = inject(NotificationService);
    get showInitials(): boolean {
        const context = this.user();
        return !!context && !context.profilePictureUrl;
    }
    get userAvatar(): string {
        const context = this.user();
        if (!context) {
            return '';
        }
        if (!context.profilePictureUrl) {
            return this.getAvatarInitials(context.fullName);
        } else {
            return context.profilePictureUrl;
        }
    }
    getAvatarInitials(name: string): string {
        const initials = name
            .split(' ')
            .map((n) => n.charAt(0).toUpperCase())
            .join('');
        return initials || '👤';
    }
    fetchUser() {
        this.userBLL.fetchUser$().subscribe({
            next: (response: Context | null) => {
                this.user.set(response);
            },
            error: (error: Error) => {
                this.notificationService.showErrorToast('Failed to fetch user data');
            }
        });
    }
}