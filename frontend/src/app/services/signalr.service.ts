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
    private userKicked$ = new Subject<{ username: string; reason: string }>();
    private kicked$ = new Subject<{ message: string }>();
    private chatRemoved$ = new Subject<{ chatRoomId: string }>();
    private error$ = new Subject<{ message: string }>();
    private connectionState$ = new BehaviorSubject<string>('disconnected');
    private pingTimerId?: number;

    constructor(private authService: AuthService) { }

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
                    this.startPing();
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
                this.stopPing()
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

    ping(): Promise<void> {
        if (!this.hubConnection) {
            return Promise.reject(new Error('Not connected to hub'));
        }
        return this.hubConnection.invoke('Ping');
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

    userKicked(): Subject<{ username: string; reason: string }> {
        return this.userKicked$;
    }

    kicked(): Subject<{ message: string }> {
        return this.kicked$;
    }

    chatRemoved(): Subject<{ chatRoomId: string }> {
        return this.chatRemoved$;
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

        this.hubConnection.on('UserKicked', (data: { username: string; reason: string }) => {
            console.log('📢 Received UserKicked event:', data);
            this.userKicked$.next(data);
        });

        this.hubConnection.on('KickedForInactivity', (data: any) => {
            const message = typeof data === 'string'
                ? data
                : (data?.message ?? 'You have been disconnected due to inactivity.');

            this.kicked$.next({ message });
        });

        this.hubConnection.on('ChatRemoved', (data: { chatRoomId: string }) => {
            this.chatRemoved$.next(data);
        });

        this.hubConnection.on('ForceDisconnect', () => {
            console.log('Forced disconnect by server');
            this.disconnect();
        });

        this.hubConnection.on('Error', (data: { message: string }) => {
            this.error$.next(data);
        });
    }
    private startPing(): void {
        this.stopPing();
        this.pingTimerId = window.setInterval(() => {
            if (!this.hubConnection) return;
            this.hubConnection.invoke('Ping').catch(() => {
            });
        }, 30_000);
    }

    private stopPing(): void {
        if (this.pingTimerId) {
            clearInterval(this.pingTimerId);
            this.pingTimerId = undefined;
        }
    }
}
