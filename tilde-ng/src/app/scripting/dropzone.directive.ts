import {Directive, EventEmitter, HostListener, Input, Output} from '@angular/core';

@Directive({
  selector: '[appDropzone]'
})
export class DropzoneDirective {

  @Input() private allowExtensions: Array<string> = [];
  // @HostBinding('style.background') private background;
  @Output() private filesChangeEmiter: EventEmitter<File[]> = new EventEmitter();

  constructor() { }

  @HostListener('dragover', ['$event']) public onDragOver(evt) {
    evt.preventDefault();
    evt.stopPropagation();

    // this.background = '#999';
  }

  @HostListener('dragleave', ['$event']) public onDragLeave(evt) {
    evt.preventDefault();
    evt.stopPropagation();

    // this.background = '#eee';
  }

  @HostListener('drop', ['$event']) public onDrop(evt) {

    evt.preventDefault();
    evt.stopPropagation();

    // this.background = '#eee';

    const files = evt.dataTransfer.files;
    const valid_files: Array<File> = [];

    if (files.length > 0) {
      for (let i = 0; i < files.length; i++) {
        const file = files[i];

        const ext = file.name.split('.')[file.name.split('.').length - 1];

        if (this.allowExtensions.lastIndexOf(ext) !== -1) {
          valid_files.push(file);
        }
      }

      this.filesChangeEmiter.emit(valid_files);
    }
  }
}
