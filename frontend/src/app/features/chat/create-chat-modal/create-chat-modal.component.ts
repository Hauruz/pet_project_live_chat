import { Component, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-create-chat-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './create-chat-modal.component.html',
  styleUrls: ['./create-chat-modal.component.scss']
})
export class CreateChatModalComponent {
  @Output() close = new EventEmitter<void>();
  @Output() create = new EventEmitter<{ title: string; type: string }>();

  chatTitle = '';
  chatType = 'Group';

  onClose(): void {
    this.close.emit();
  }

  onCreate(): void {
    if (!this.chatTitle.trim()) {
      alert('Chat room name is required');
      return;
    }

    this.create.emit({
      title: this.chatTitle,
      type: this.chatType
    });

    this.chatTitle = '';
    this.chatType = 'Group';
  }
}
