import { ComponentFixture } from '@angular/core/testing';

export function setInputValue<T>(
  fixture: ComponentFixture<T>,
  selector: string,
  value: string,
): void {
  const input = fixture.nativeElement.querySelector(selector) as HTMLInputElement;
  input.value = value;
  input.dispatchEvent(new Event('input'));
  fixture.detectChanges();
}

export function selectOption<T>(
  fixture: ComponentFixture<T>,
  selector: string,
  index: number,
): void {
  const select = fixture.nativeElement.querySelector(selector) as HTMLSelectElement;
  select.selectedIndex = index;
  select.dispatchEvent(new Event('change'));
  fixture.detectChanges();
}

export function submitForm<T>(fixture: ComponentFixture<T>): void {
  const form = fixture.nativeElement.querySelector('form') as HTMLFormElement;
  form.dispatchEvent(new Event('submit', { cancelable: true }));
  fixture.detectChanges();
}
