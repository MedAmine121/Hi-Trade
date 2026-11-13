import { BaseDTO } from '../common/base-dto.model';

export interface AdminUserDTO extends BaseDTO {
  fullName: string;
  email: string;
  address: string;
  balance: number;
  role: string;
  portfoliosCount: number;
  createdAt: string | Date;
  enabled: boolean;
}
