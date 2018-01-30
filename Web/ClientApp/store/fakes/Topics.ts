export const Topics = [{
    id: 1,
    name: "Téma 1",
    children: [{
        id: 11,
        parentTopicId: 1,
        name: "Téma 1.1",
        children: [{
            id: 111,
            parentTopicId: 11,
            name: "Téma 1.1.1",
            children: []
        }, {
            id: 112,
            parentTopicId: 11,
            name: "Téma 1.1.2",
            children: []
        }, {
            id: 113,
            parentTopicId: 11,
            name: "Téma 1.1.3",
            children: []
        }]
    }, {
        id: 12,
        parentTopicId: 1,
        name: "Téma 1.2",
        children: [{
            id: 121,
            parentTopicId: 12,
            name: "Téma 1.2.1",
            children: []
        }, {
            id: 122,
            parentTopicId: 12,
            name: "Téma 1.2.2",
            children: []
        }]
    }]
}, {
    id: 2,
    name: "Téma 2",
    children: [{
        id: 21,
        parentTopicId: 2,
        name: "Téma 2.1",
        children: [{
            id: 211,
            parentTopicId: 21,
            name: "Téma 2.1.1",
            children: []
        }, {
            id: 212,
            parentTopicId: 21,
            name: "Téma 2.1.2",
            children: []
        }]
    }]
}]