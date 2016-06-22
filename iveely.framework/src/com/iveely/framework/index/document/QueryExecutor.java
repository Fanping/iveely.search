package com.iveely.framework.index.document;

import org.apache.lucene.analysis.Analyzer;
import org.apache.lucene.analysis.standard.StandardAnalyzer;
import org.apache.lucene.document.Document;
import org.apache.lucene.index.DirectoryReader;
import org.apache.lucene.index.IndexReader;
import org.apache.lucene.queryparser.classic.ParseException;
import org.apache.lucene.queryparser.classic.QueryParser;
import org.apache.lucene.search.IndexSearcher;
import org.apache.lucene.search.Query;
import org.apache.lucene.search.ScoreDoc;
import org.apache.lucene.search.TopScoreDocCollector;
import org.apache.lucene.store.FSDirectory;

import java.io.IOException;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.List;

/**
 * author { liufanping@iveely.com }.
 */
public class QueryExecutor {

  protected IndexReader reader;

  protected IndexSearcher searcher;

  protected Analyzer analyzer;

  public QueryExecutor(final String indexPath) throws IOException {
    this.reader = DirectoryReader.open(FSDirectory.open(Paths.get(indexPath)));
    this.searcher = new IndexSearcher(reader);
    this.analyzer = new StandardAnalyzer();
  }

  public List<StoredDocument> find(final String queryField,
                                   final String[] respFields,
                                   final String keywords,
                                   final int totalHits)
      throws ParseException, IOException {
    List<StoredDocument> responses = new ArrayList<>();

    Query query = new QueryParser(queryField, analyzer).parse(keywords);
    TopScoreDocCollector collector = TopScoreDocCollector.create(totalHits);
    searcher.search(query, collector);
    ScoreDoc[] hits = collector.topDocs().scoreDocs;
    for (int i = 0; i < hits.length; i++) {
      Document doc = searcher.doc(hits[i].doc);
      List<StoreField> list = new ArrayList<>();
      for (String field : respFields) {
        String fieldValue = doc.get(field);
        if (fieldValue == null || fieldValue.length() < 1) {
          continue;
        }
        StoreField<String> storeField = new StoreField<>(StoreField.Type
            .STRING, field, fieldValue);
        list.add(storeField);
      }
      if (list.size() > 0) {
        responses.add(new StoredDocument(list));
      }
    }
    return responses;
  }
}
