import { Injectable } from '@angular/core';
import {
  HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { ToastService } from '../services/toast.service';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  constructor(private toast: ToastService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      catchError((err: HttpErrorResponse) => {
        let msg = 'Ocorreu um erro inesperado.';
        if (err.status === 0)                msg = 'Sem conexão com o servidor.';
        else if (err.status === 400)         msg = err.error?.message ?? 'Dados inválidos.';
        else if (err.status === 403)         msg = 'Sem permissão para esta ação.';
        else if (err.status === 404)         msg = 'Recurso não encontrado.';
        else if (err.status === 409)         msg = err.error?.message ?? 'Conflito de dados.';
        else if (err.status === 422)         msg = err.error?.message ?? 'Dados inválidos.';
        else if (err.status >= 500)          msg = 'Erro no servidor. Tente novamente.';

        // Não mostrar toast para 401 (tratado pelo AuthInterceptor)
        if (err.status !== 401) this.toast.error(msg);

        return throwError(() => err);
      })
    );
  }
}
