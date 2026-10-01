import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

@Component({ selector: 'app-order-confirmation', templateUrl: './order-confirmation.component.html', styleUrls: ['./order-confirmation.component.css'] })
export class OrderConfirmationComponent implements OnInit {
  type: 'sucesso' | 'falha' | 'pendente' = 'sucesso';
  orderId: string | null = null;

  configs = {
    sucesso: {
      icon: '✅', title: 'Pedido confirmado!',
      message: 'Seu pagamento foi aprovado. Você receberá as atualizações do pedido.',
      btnLabel: 'Ver meu pedido', btnRoute: '/minha-conta/pedidos', color: 'text-green-600'
    },
    pendente: {
      icon: '⏳', title: 'Pagamento em processamento',
      message: 'Seu pagamento está sendo processado. Você será notificado em breve.',
      btnLabel: 'Acompanhar pedido', btnRoute: '/minha-conta/pedidos', color: 'text-yellow-600'
    },
    falha: {
      icon: '❌', title: 'Pagamento não aprovado',
      message: 'Não foi possível processar seu pagamento. Tente novamente ou escolha outra forma.',
      btnLabel: 'Tentar novamente', btnRoute: '/carrinho', color: 'text-red-600'
    }
  };

  get config() { return this.configs[this.type]; }

  constructor(private route: ActivatedRoute, private router: Router) {}

  ngOnInit(): void {
    const path = this.router.url.split('/').pop()?.split('?')[0] as any;
    if (path === 'falha' || path === 'pendente') this.type = path;
    this.orderId = this.route.snapshot.queryParams['orderId'] ?? null;
  }
}
