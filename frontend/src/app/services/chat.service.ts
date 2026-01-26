import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ChatRoomDto, CreateChatRoomRequest, MessageDto } from '../models';

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private apiUrl = 'http://localhost:5055/api/chat';

  constructor(private http: HttpClient) {}

  createChatRoom(request: CreateChatRoomRequest): Observable<ChatRoomDto> {
    return this.http.post<ChatRoomDto>(`${this.apiUrl}/rooms`, request);
  }

  getUserChatRooms(): Observable<ChatRoomDto[]> {
    return this.http.get<ChatRoomDto[]>(`${this.apiUrl}/rooms`);
  }

  getChatRoom(id: string): Observable<ChatRoomDto> {
    return this.http.get<ChatRoomDto>(`${this.apiUrl}/rooms/${id}`);
  }

  getChatMessages(id: string, limit: number = 50): Observable<MessageDto[]> {
    return this.http.get<MessageDto[]>(`${this.apiUrl}/rooms/${id}/messages?limit=${limit}`);
  }

  deleteChatRoom(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/rooms/${id}`);
  }

  inviteUserToChatRoom(roomId: string, username: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/rooms/${roomId}/invite`, { username });
  }
}
