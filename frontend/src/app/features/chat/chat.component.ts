import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ChatRoomListComponent } from './chat-room-list/chat-room-list.component';
import { ChatRoomComponent } from './chat-room/chat-room.component';
import { CreateChatModalComponent } from './create-chat-modal/create-chat-modal.component';
import { ChatService } from '../../services/chat.service';
import { SignalRService } from '../../services/signalr.service';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { ChatRoomDto } from '../../models';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule, ChatRoomListComponent, ChatRoomComponent, CreateChatModalComponent],
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.scss']
})
export class ChatComponent implements OnInit, OnDestroy {
  selectedChatRoom: ChatRoomDto | null = null;
  chatRooms: ChatRoomDto[] = [];
  loading = true;
  showCreateModal = false;
  currentUserId: string | null = null;

  constructor(
    private chatService: ChatService,
    private signalRService: SignalRService,
    public authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    const user = this.authService.getCurrentUser();
    if (user) {
      this.currentUserId = user.userId;
    }
    this.initializeChat();
  }

  async initializeChat(): Promise<void> {
    try {
      await this.signalRService.connect();
      this.loadChatRooms();
    } catch (error) {
      console.error('Failed to connect to chat:', error);
      this.router.navigate(['/login']);
    }
  }

  loadChatRooms(): void {
    this.loading = true;
    this.chatService.getUserChatRooms().subscribe({
      next: (rooms) => {
        this.chatRooms = rooms;
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load chat rooms:', err);
        this.loading = false;
      }
    });
  }

  selectChatRoom(chatRoom: ChatRoomDto): void {
    this.selectedChatRoom = chatRoom;
  }

  onCreateChatRoom(): void {
    this.showCreateModal = true;
  }

  onCloseCreateModal(): void {
    this.showCreateModal = false;
  }

  onCreateChat(data: { title: string; type: string }): void {
    const currentUser = this.authService.getCurrentUser();
    if (!currentUser) {
      alert('User not authenticated');
      return;
    }

    const createRequest = {
      type: data.type,
      title: data.title || 'Untitled Chat',
      memberIds: [currentUser.userId]
    };

    console.log('Creating chat room with:', createRequest);

    this.chatService.createChatRoom(createRequest).subscribe({
      next: (newRoom) => {
        console.log('Chat room created:', newRoom);
        this.chatRooms.push(newRoom);
        this.selectedChatRoom = newRoom;
        this.showCreateModal = false;
      },
      error: (err) => {
        console.error('Failed to create chat room:', err);
        console.error('Error details:', err.error);
        alert('Failed to create chat room: ' + (err.error?.message || err.statusText));
      }
    });
  }

  onDeleteChatRoom(roomId: string): void {
    this.chatService.deleteChatRoom(roomId).subscribe({
      next: () => {
        console.log('Chat room deleted:', roomId);
        this.chatRooms = this.chatRooms.filter(room => room.id !== roomId);
        if (this.selectedChatRoom?.id === roomId) {
          this.selectedChatRoom = this.chatRooms.length > 0 ? this.chatRooms[0] : null;
        }
      },
      error: (err) => {
        console.error('Failed to delete chat room:', err);
        alert('Failed to delete chat room: ' + (err.error?.message || 'Unknown error'));
      }
    });
  }

  logout(): void {
    this.authService.logout();
    this.signalRService.disconnect();
    this.router.navigate(['/login']);
  }

  ngOnDestroy(): void {
    this.signalRService.disconnect();
  }
}
