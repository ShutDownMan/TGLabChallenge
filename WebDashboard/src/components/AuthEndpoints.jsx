import React, { useState } from 'react';
import axios from 'axios';
import { Card, CardContent, Typography, TextField, Button, Grid } from '@mui/material';
import ResponseDisplay from './ResponseDisplay';
import { API_HOST } from '../config/api';

function AuthEndpoints({ jwtToken, setJwtToken }) {
  const [response, setResponse] = useState(null);
  const [status, setStatus] = useState(null);
  const [responseTime, setResponseTime] = useState(null);
  const [username, setUsername] = useState('testuser');
  const [password, setPassword] = useState('password123');
  const [email, setEmail] = useState('testuser@example.com');

  const handleRequest = async (endpoint, method, data = null) => {
    try {
      const startTime = performance.now();
      const config = {
        method,
        url: `${API_HOST}${endpoint}`,
        headers: { Authorization: `Bearer ${jwtToken}` },
        data,
      };
      const res = await axios(config);
      const endTime = performance.now();
      setResponse(res.data);
      setStatus(res.status);
      setResponseTime(Math.round(endTime - startTime));
      if (endpoint === '/api/Auth/login' && method === 'POST') {
        setJwtToken(res.data.token); // Automatically set token on login success
      }
    } catch (error) {
      setResponse(error.response?.data || error.message);
      setStatus(error.response?.status || 'Error');
      setResponseTime(null);
    }
  };

  return (
    <Card>
      <CardContent>
        <Typography variant="h6" gutterBottom>
          Auth Endpoints
        </Typography>
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <TextField
              label="Username"
              fullWidth
              value={username}
              onChange={(e) => setUsername(e.target.value)}
            />
          </Grid>
          <Grid item xs={12}>
            <TextField
              label="Password"
              fullWidth
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
            />
          </Grid>
          <Grid item xs={12}>
            <TextField
              label="Email"
              fullWidth
              value={email}
              onChange={(e) => setEmail(e.target.value)}
            />
          </Grid>
          <Grid item xs={12}>
            <Button
              variant="contained"
              color="primary"
              onClick={() =>
                handleRequest('/api/Auth/register', 'POST', {
                  username,
                  password,
                  email,
                  currencyId: 1,
                  initialBalance: '1000.00',
                })
              }
            >
              Register
            </Button>
          </Grid>
          <Grid item xs={12}>
            <Button
              variant="contained"
              color="secondary"
              onClick={() =>
                handleRequest('/api/Auth/login', 'POST', { identifier: username, password })
              }
            >
              Login
            </Button>
          </Grid>
        </Grid>
        <ResponseDisplay response={response} status={status} responseTime={responseTime} />
      </CardContent>
    </Card>
  );
}

export default AuthEndpoints;
