import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject, BehaviorSubject } from 'rxjs';
import { AuthService } from './auth.service';
import { MessageDto } from '../models';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection?: signalR.HubConnection;
  private messageReceived$ = new Subject<MessageDto>();
  private userJoined$ = new Subject<{ userId: string; message: string }>();
  private userLeft$ = new Subject<{ userId: string; message: string }>();
  private error$ = new Subject<{ message: string }>();
  private connectionState$ = new BehaviorSubject<string>('disconnected');

  constructor(private authService: AuthService) {}

  connect(): Promise<void> {
    return new Promise((resolve, reject) => {
      const token = this.authService.getToken();
      
      if (!token) {
        reject(new Error('No authentication token available'));
        return;
      }

      this.hubConnection = new signalR.HubConnectionBuilder()
        .withUrl('http://localhost:5055/hubs/chat', {
          accessTokenFactory: () => token
        })
        .withAutomaticReconnect()
        .build();

      this.setupListeners();

      this.hubConnection.start()
        .then(() => {
          console.log('Connected to chat hub');
          this.connectionState$.next('connected');
          resolve();
        })
        .catch(err => {
          console.error('Error connecting to chat hub:', err);
          this.connectionState$.next('disconnected');
          reject(err);
        });
    });
  }

  disconnect(): Promise<void> {
    return new Promise((resolve, reject) => {
      if (this.hubConnection) {
        this.hubConnection.stop()
          .then(() => {
            console.log('Disconnected from chat hub');
            this.connectionState$.next('disconnected');
            resolve();
          })
          .catch(err => {
            console.error('Error disconnecting from chat hub:', err);
            reject(err);
          });
      } else {
        resolve();
      }
    });
  }

  joinChatRoom(chatRoomId: string): Promise<void> {
    if (!this.hubConnection) {
      return Promise.reject(new Error('Not connected to hub'));
    }
    return this.hubConnection.invoke('JoinChatRoom', chatRoomId);
  }

  leaveChatRoom(chatRoomId: string): Promise<void> {
    if (!this.hubConnection) {
      return Promise.reject(new Error('Not connected to hub'));
    }
    return this.hubConnection.invoke('LeaveChatRoom', chatRoomId);
  }

  sendMessage(chatRoomId: string, text: string): Promise<void> {
    if (!this.hubConnection) {
      return Promise.reject(new Error('Not connected to hub'));
    }
    return this.hubConnection.invoke('SendMessage', chatRoomId, text);
  }

  getChatHistory(chatRoomId: string, limit: number = 50): Promise<MessageDto[]> {
    if (!this.hubConnection) {
      return Promise.reject(new Error('Not connected to hub'));
    }
    return this.hubConnection.invoke<MessageDto[]>('GetChatHistory', chatRoomId, limit);
  }

  messageReceived(): Subject<MessageDto> {
    return this.messageReceived$;
  }

  userJoined(): Subject<{ userId: string; message: string }> {
    return this.userJoined$;
  }

  userLeft(): Subject<{ userId: string; message: string }> {
    return this.userLeft$;
  }

  error(): Subject<{ message: string }> {
    return this.error$;
  }

  connectionState(): BehaviorSubject<string> {
    return this.connectionState$;
  }

  private setupListeners(): void {
    if (!this.hubConnection) return;

    this.hubConnection.on('ReceiveMessage', (message: MessageDto) => {
      this.messageReceived$.next(message);
    });

    this.hubConnection.on('UserJoined', (data: { userId: string; message: string }) => {
      this.userJoined$.next(data);
    });

    this.hubConnection.on('UserLeft', (data: { userId: string; message: string }) => {
      this.userLeft$.next(data);
    });

    this.hubConnection.on('Error', (data: { message: string }) => {
      this.error$.next(data);
    });
  }
}
