const http = require('http');

const PORT = 5050;

const MOCK_USER = {
  userId: "user-123",
  cursorId: "cursor-123",
  username: "admin",
  firstName: "Admin",
  lastName: "User",
  email: "admin@example.com",
  role: "Admin",
  token: "mock-jwt-token"
};

const MOCK_DASHBOARD = {
  totalCompanies: 15,
  totalUsers: 42,
  activeUsers: 30,
  totalArticles: 120,
  totalSuppliers: 8,
  totalCustomers: 25,
  generatedAt: new Date().toISOString(),
};

const MOCK_COMPANIES = [
  {
    id: 'c1',
    name: 'Organización Demo 1',
    taxId: 'B12345678',
    address: 'Calle Principal 1',
    phone: '555-0001',
    email: 'contacto@demo1.com',
    isActive: true,
  },
  {
    id: 'c2',
    name: 'Organización Demo 2',
    taxId: 'B87654321',
    address: 'Avenida Secundaria 2',
    phone: '555-0002',
    email: 'contacto@demo2.com',
    isActive: false,
  },
];

const server = http.createServer((req, res) => {
  // CORS headers
  res.setHeader('Access-Control-Allow-Origin', '*');
  res.setHeader('Access-Control-Allow-Methods', 'GET, POST, OPTIONS');
  res.setHeader('Access-Control-Allow-Headers', 'Content-Type, Authorization');

  if (req.method === 'OPTIONS') {
    res.writeHead(204);
    res.end();
    return;
  }

  const url = new URL(req.url, `http://${req.headers.host}`);
  const path = url.pathname;

  console.log(`${req.method} ${path}`);

  // Login
  if (path === '/api/admin/auth/login' && req.method === 'POST') {
    let body = '';
    req.on('data', chunk => body += chunk);
    req.on('end', () => {
      // Accept any credentials for test
      res.writeHead(200, { 'Content-Type': 'application/json' });
      res.end(JSON.stringify(MOCK_USER));
    });
    return;
  }

  // Dashboard
  if (path === '/api/admin/dashboard/summary' && req.method === 'GET') {
    res.writeHead(200, { 'Content-Type': 'application/json' });
    res.end(JSON.stringify(MOCK_DASHBOARD));
    return;
  }

  // Companies List
  if (path === '/company' && req.method === 'GET') {
    res.writeHead(200, { 'Content-Type': 'application/json' });
    res.end(JSON.stringify(MOCK_COMPANIES));
    return;
  }

  // Company Create
  if (path === '/company' && req.method === 'POST') {
    let body = '';
    req.on('data', chunk => body += chunk);
    req.on('end', () => {
      const data = JSON.parse(body);
      const newCompany = { ...data, id: `new-${Date.now()}`, isActive: true };
      res.writeHead(201, { 'Content-Type': 'application/json' });
      res.end(JSON.stringify(newCompany));
    });
    return;
  }

  // 404
  res.writeHead(404);
  res.end(JSON.stringify({ error: 'Not Found' }));
});

server.listen(PORT, () => {
  console.log(`Mock API running on port ${PORT}`);
});
