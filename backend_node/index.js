import mongoose from 'mongoose';
import bodyParser from 'body-parser';
import express from 'express';
import { addContact, getAllContacts, getContactById, deleteContact, syncAllContacts } from './src/controllers/apiController';
const cors = require('cors');

const app = express();
const PORT = 8081;

mongoose.Promise = global.Promise;
mongoose.connect('mongodb://localhost/apiDb', {
    useNewUrlParser: true
});

app.use(bodyParser.urlencoded({ extended: true}));
app.use(bodyParser.json());

app.use(cors({
    origin: '*',
    methods: ['GET','POST','DELETE']
}));

const swaggerUi = require('swagger-ui-express'),
    swaggerDocument = require('./swagger.json');

app.route('/contacts/sync')
    .get(syncAllContacts)

app.route('/contacts')
    .get(getAllContacts)

    .post(addContact)

app.route('/contacts/:contactId')
    .get(getContactById)

    .delete(deleteContact)

app.get('/', (req, res) => 
    res.send(`Node and express server is running on port ${PORT}`)
);

app.use('/swagger', swaggerUi.serve, swaggerUi.setup(swaggerDocument));

app.listen(PORT, () => 
    console.log(`Your server is running on port ${PORT}`)
);