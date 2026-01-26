import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ChatRoomDto } from '../../../models';

@Component({
  selector: 'app-chat-room-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './chat-room-list.component.html',
  styleUrls: ['./chat-room-list.component.scss']
})
export class ChatRoomListComponent {
  @Input() chatRooms: ChatRoomDto[] = [];
  @Input() selectedChatRoom: ChatRoomDto | null = null;
  @Input() loading = false;
  @Input() currentUserId: string | null = null;
  @Output() selectChatRoom = new EventEmitter<ChatRoomDto>();
  @Output() createChatRoom = new EventEmitter<void>();
  @Output() deleteChatRoom = new EventEmitter<string>();

  onSelectChatRoom(chatRoom: ChatRoomDto): void {
    this.selectChatRoom.emit(chatRoom);
  }

  onCreateChatRoom(): void {
    this.createChatRoom.emit();
  }

  onDeleteChatRoom(event: Event, chatRoom: ChatRoomDto): void {
    event.stopPropagation();
    if (confirm(`Are you sure you want to delete "${this.getChatRoomTitle(chatRoom)}"?`)) {
      this.deleteChatRoom.emit(chatRoom.id);
    }
  }

  getChatRoomTitle(chatRoom: ChatRoomDto): string {
    if (chatRoom.title) return chatRoom.title;
    return chatRoom.memberUsernames.join(', ');
  }

  isCreator(chatRoom: ChatRoomDto): boolean {
    return chatRoom.createdByUserId === this.currentUserId;
  }
}
