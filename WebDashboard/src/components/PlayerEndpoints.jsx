import React, { useState } from 'react';
import axios from 'axios';
import { API_HOST } from '../config/api';
import { Card, CardContent, Typography, Button, Grid } from '@mui/material';
import ResponseDisplay from './ResponseDisplay';

function PlayerEndpoints({ jwtToken }) {
  const [response, setResponse] = useState(null);
  const [status, setStatus] = useState(null);
  const [responseTime, setResponseTime] = useState(null);

  const handleRequest = async (endpoint, method) => {
    try {
      const startTime = performance.now();
      const config = {
        method,
        url: `${API_HOST}${endpoint}`,
        headers: { Authorization: `Bearer ${jwtToken}` },
      };
      const res = await axios(config);
      const endTime = performance.now();
      setResponse(res.data);
      setStatus(res.status);
      setResponseTime(Math.round(endTime - startTime));
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
          Player Endpoints
        </Typography>
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <Button
              variant="contained"
              color="primary"
              onClick={() => handleRequest('/api/Player/profile', 'GET')}
            >
              Get Player Profile
            </Button>
          </Grid>
          <Grid item xs={12}>
            <Button
              variant="contained"
              color="secondary"
              onClick={() => handleRequest('/api/Player/bets', 'GET')}
            >
              Get Player Bets
            </Button>
          </Grid>
          <Grid item xs={12}>
            <Button
              variant="contained"
              color="success"
              onClick={() => handleRequest('/api/Player/wallet-transactions', 'GET')}
            >
              Get Wallet Transactions
            </Button>
          </Grid>
        </Grid>
        <ResponseDisplay response={response} status={status} responseTime={responseTime} />
      </CardContent>
    </Card>
  );
}

export default PlayerEndpoints;
