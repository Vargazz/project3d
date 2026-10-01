import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'currencyBrl' })
export class CurrencyBrlPipe implements PipeTransform {
  transform(value: number | undefined | null): string {
    if (value == null) return 'R$ 0,00';
    return value.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
  }
}
