export interface Wallet {
  userId: string;
  balance: number;
  currency: string;
  recentTransactions: Transaction[];
}

export interface Transaction {
  id: string;
  userId: string;
  type: string; // Deposit, Withdrawal, Hold, Release
  amount: number;
  description: string;
  timestamp: string;
}

export interface DepositRequest {
  amount: number;
  paymentMethod: string; // CreditCard, BankTransfer, etc.
  paymentDetails: string; // JSON or other format containing payment details
}

export interface WithdrawalRequest {
  amount: number;
  withdrawalMethod: string; // BankTransfer, PayPal, etc.
  withdrawalDetails: string; // Account details, etc.
}

export interface TransferFundsRequest {
  recipientId: string;
  amount: number;
  description: string;
}

export interface TransactionHistoryResponse {
  userId: string;
  transactions: Transaction[];
  totalCount: number;
  page: number;
  pageSize: number;
}
