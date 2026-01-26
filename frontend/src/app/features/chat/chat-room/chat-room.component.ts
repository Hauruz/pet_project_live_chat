import { Component, Input, OnInit, OnDestroy, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChatRoomDto, MessageDto } from '../../../models';
import { SignalRService } from '../../../services/signalr.service';
import { AuthService } from '../../../services/auth.service';
import { ChatService } from '../../../services/chat.service';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-chat-room',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat-room.component.html',
  styleUrls: ['./chat-room.component.scss']
})
export class ChatRoomComponent implements OnInit, OnDestroy, AfterViewChecked {
  @Input() chatRoom: ChatRoomDto | null = null;
  @ViewChild('messagesContainer') private messagesContainer?: ElementRef;

  messages: MessageDto[] = [];
  messageText = '';
  inviteUsername = '';
  sending = false;
  loading = true;
  inviting = false;
  showInviteForm = false;
  private destroy$ = new Subject<void>();
  private shouldScroll = false;

  constructor(
    private signalRService: SignalRService,
    private authService: AuthService,
    private chatService: ChatService
  ) {}

  ngOnInit(): void {
    if (!this.chatRoom) return;

    this.loadChatHistory();
    this.subscribeToMessages();
  }

  loadChatHistory(): void {
    if (!this.chatRoom) return;

    this.signalRService.getChatHistory(this.chatRoom.id).then(
      (history) => {
        this.messages = history;
        this.loading = false;
        this.shouldScroll = true;
      },
      (error) => {
        console.error('Failed to load chat history:', error);
        this.loading = false;
      }
    );
  }

  subscribeToMessages(): void {
    if (!this.chatRoom) return;

    this.signalRService.joinChatRoom(this.chatRoom.id).then(() => {
      this.signalRService.messageReceived()
        .pipe(takeUntil(this.destroy$))
        .subscribe((message) => {
          this.messages.push(message);
          this.shouldScroll = true;
        });

      this.signalRService.userJoined()
        .pipe(takeUntil(this.destroy$))
        .subscribe((data) => {
          console.log(data.message);
        });

      this.signalRService.error()
        .pipe(takeUntil(this.destroy$))
        .subscribe((data) => {
          console.error(data.message);
        });
    });
  }

  sendMessage(): void {
    if (!this.chatRoom || !this.messageText.trim() || this.sending) return;

    this.sending = true;
    const text = this.messageText.trim();
    this.messageText = '';

    this.signalRService.sendMessage(this.chatRoom.id, text).then(() => {
      this.sending = false;
    }, (error) => {
      console.error('Failed to send message:', error);
      this.messageText = text;
      this.sending = false;
    });
  }

  inviteUser(): void {
    if (!this.chatRoom || !this.inviteUsername.trim() || this.inviting) return;

    this.inviting = true;
    const username = this.inviteUsername.trim();
    this.inviteUsername = '';

    this.chatService.inviteUserToChatRoom(this.chatRoom.id, username).subscribe({
      next: () => {
        console.log(`User ${username} invited successfully`);
        alert(`${username} has been invited to the chat!`);
        this.inviting = false;
        this.showInviteForm = false;
      },
      error: (err) => {
        console.error('Failed to invite user:', err);
        alert('Failed to invite user: ' + (err.error?.message || err.statusText));
        this.inviteUsername = username;
        this.inviting = false;
      }
    });
  }

  getChatRoomTitle(): string {
    if (!this.chatRoom) return '';
    if (this.chatRoom.title) return this.chatRoom.title;
    return this.chatRoom.memberUsernames.join(', ');
  }

  isCurrentUserMessage(message: MessageDto): boolean {
    return message.senderId === this.authService.getCurrentUser()?.userId;
  }

  ngAfterViewChecked(): void {
    if (this.shouldScroll) {
      this.scrollToBottom();
      this.shouldScroll = false;
    }
  }

  private scrollToBottom(): void {
    try {
      if (this.messagesContainer) {
        this.messagesContainer.nativeElement.scrollTop = 
          this.messagesContainer.nativeElement.scrollHeight;
      }
    } catch (err) {
      console.error('Error scrolling to bottom:', err);
    }
  }

  ngOnDestroy(): void {
    if (this.chatRoom) {
      this.signalRService.leaveChatRoom(this.chatRoom.id);
    }
    this.destroy$.next();
    this.destroy$.complete();
  }
}
