import React from 'react';
import { Card, CardContent, Typography } from '@mui/material';

function ResponseDisplay({ response, status, responseTime }) {
  return (
    <Card style={{ marginTop: '20px' }}>
      <CardContent>
        <Typography variant="h6" gutterBottom>
          Response
        </Typography>
        <Typography variant="body1">
          <strong>Status:</strong> {status || 'N/A'}
        </Typography>
        <Typography variant="body1">
          <strong>Response Time:</strong> {responseTime ? `${responseTime} ms` : 'N/A'}
        </Typography>
        <Typography variant="body1" style={{ marginTop: '10px' }}>
          <strong>Data:</strong>
        </Typography>
        <pre>{JSON.stringify(response, null, 2)}</pre>
      </CardContent>
    </Card>
  );
}

export default ResponseDisplay;
