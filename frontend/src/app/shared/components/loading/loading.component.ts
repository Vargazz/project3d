import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-loading',
  template: `
    <div [class]="'flex items-center justify-center ' + containerClass">
      <div [class]="'border-2 border-gray-200 border-t-primary-500 rounded-full animate-spin ' + sizeClass"></div>
      <span *ngIf="text" class="ml-2 text-gray-500 text-sm">{{text}}</span>
    </div>
  `
})
export class LoadingComponent {
  @Input() size: 'sm' | 'md' | 'lg' = 'md';
  @Input() text = '';
  @Input() fullPage = false;

  get sizeClass(): string {
    return { sm: 'w-4 h-4', md: 'w-7 h-7', lg: 'w-10 h-10' }[this.size];
  }
  get containerClass(): string {
    return this.fullPage ? 'min-h-[60vh]' : 'py-8';
  }
}
