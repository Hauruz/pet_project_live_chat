export interface AuthResponse {
  token: string;
  username: string;
  userId: string;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  password: string;
}

export interface MessageDto {
  id: string;
  chatRoomId: string;
  senderId: string;
  senderUsername: string;
  text: string;
  createdAt: Date;
}

export interface ChatRoomDto {
  id: string;
  type: string;
  title?: string;
  createdAt: Date;
  createdByUserId: string;
  memberUsernames: string[];
  messages: MessageDto[];
}

export interface CreateChatRoomRequest {
  type: string;
  title?: string;
  memberIds: string[];
}
