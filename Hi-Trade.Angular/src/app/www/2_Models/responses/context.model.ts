import { Roles } from "../../6_Common/roles.enum";

export interface Context {
    id: number;
    email: string;
    password: string;
    fullName: string;
    address: string;
    profilePictureUrl: string;
    balance: number;
    role: Roles;
    token: string;
    expires: Date;
}