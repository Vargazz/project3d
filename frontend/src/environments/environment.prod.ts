export const environment = {
  production: true,
  // Em produção o nginx faz proxy /api → api:5000/api
  // então podemos usar caminhos relativos
  apiUrl: '/api',
  uploadsUrl: '/uploads'
};
