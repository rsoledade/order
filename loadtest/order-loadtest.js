import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
    vus: 20, // virtual users (adjust as needed)
    duration: '1h', // test duration (adjust as needed)
    thresholds: {
        http_req_duration: ['p(95)<2000'], // 95% das requisições em menos de 2s
        http_req_failed: ['rate<0.01'],    // menos de 1% de falhas
    },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';
const ENDPOINT = '/api/orders/register-order';

function randomOrder() {
    const guid = crypto.randomUUID();
    return {
        externalId: guid,
        products: [
            {
                name: `Product ${Math.floor(Math.random() * 1000)}`,
                price: Math.floor(Math.random() * 100) + 1,
                quantity: Math.floor(Math.random() * 5) + 1
            }
        ]
    };
}

export default function () {
    const payload = JSON.stringify(randomOrder());
    const params = {
        headers: {
            'Content-Type': 'application/json',
        },
    };

    const res = http.post(`${BASE_URL}${ENDPOINT}`, payload, params);

    check(res, {
        'status is 201 or 200': (r) => r.status === 201 || r.status === 200,
    });

    // Ajuste o sleep para controlar o throughput desejado
    sleep(0.5); // 2 req/s por VU (ajuste conforme necessário)
}
