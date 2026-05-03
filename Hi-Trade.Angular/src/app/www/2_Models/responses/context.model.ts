import { Roles } from "../../6_Common/roles.enum";
import { BaseDTO } from "../common/base-dto.model";

export interface Context extends BaseDTO {
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